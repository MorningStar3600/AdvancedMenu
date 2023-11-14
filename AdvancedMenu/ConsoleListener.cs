using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace MenuSystem
{
    static class ConsoleListener {
        static Thread thread;
        static int width, height;
        
        public static bool Run()
        {
            if (thread is not null) return false;
            width = Console.WindowWidth;
            height = Console.WindowHeight;
            thread = new Thread(EventHandling);
            thread.Start();

            return true;


        }

        private static void EventHandling()
        {
            var handle = NativeMethods.GetStdHandle(NativeMethods.StdInputHandle);

                int mode = 0;
                if (!(NativeMethods.GetConsoleMode(handle, ref mode))) { throw new Win32Exception(); }

                mode |= NativeMethods.EnableMouseInput;
                mode &= ~NativeMethods.EnableQuickEditMode;
                mode |= NativeMethods.EnableExtendedFlags;

                if (!(NativeMethods.SetConsoleMode(handle, mode))) { throw new Win32Exception(); }

                var record = new NativeMethods.InputRecord();
                uint recordLen = 0;
                while (thread is not null) {
                    if (Console.WindowWidth != width || Console.WindowHeight != height) {
                        width = Console.WindowWidth;
                        height = Console.WindowHeight;
                        if (EventHandler.IsWaitingForCustomAction())
                        {
                            Console.Clear();
                            EventHandler.GetCustomDraw()?.Invoke();
                        }
                        else
                        {
                            Console.Clear();
                            Menu.Draw();
                        }
                        
                    }
                    if (!(NativeMethods.ReadConsoleInput(handle, ref record, 1, ref recordLen))) { throw new Win32Exception(); }
                    switch (record.EventType) {
                        case NativeMethods.MouseEvent:
                        {
                            EventHandler.MouseHandler(record.MouseEvent.dwMousePosition.X, record.MouseEvent.dwMousePosition.Y, record.MouseEvent.dwEventFlags, record.MouseEvent.dwButtonState);


                            /*Console.WriteLine("Mouse event");
                            Console.WriteLine(string.Format("    X ...............:   {0,4:0}  ", record.MouseEvent.dwMousePosition.X));
                            Console.WriteLine(string.Format("    Y ...............:   {0,4:0}  ", record.MouseEvent.dwMousePosition.Y));
                            Console.WriteLine(string.Format("    dwButtonState ...: 0x{0:X4}  ", record.MouseEvent.dwButtonState));
                            Console.WriteLine(string.Format("    dwControlKeyState: 0x{0:X4}  ", record.MouseEvent.dwControlKeyState));
                            Console.WriteLine(string.Format("    dwEventFlags ....: 0x{0:X4}  ", record.MouseEvent.dwEventFlags));*/
                        } break;

                    case NativeMethods.KeyEvent:
                    {
                        if (record.KeyEvent.bKeyDown && record.KeyEvent.wVirtualKeyCode != (int) ConsoleKey.Escape)
                        {
                            EventHandler.KeyHandler(record.KeyEvent.UnicodeChar, record.KeyEvent.wVirtualKeyCode);
                        }
                        
                       /*Console.WriteLine("Key event  ");
                       Console.WriteLine(string.Format("    bKeyDown  .......:  {0,5}  ", record.KeyEvent.bKeyDown)); Console.WriteLine(string.Format("    wRepeatCount ....:   {0,4:0}  ", record.KeyEvent.wRepeatCount));
                       Console.WriteLine(string.Format("    wVirtualKeyCode .:   {0,4:0}  ", record.KeyEvent.wVirtualKeyCode));
                       Console.WriteLine(string.Format("    uChar ...........:      {0}  ", record.KeyEvent.UnicodeChar));
                       Console.WriteLine(string.Format("    dwControlKeyState: 0x{0:X4}  ", record.KeyEvent.dwControlKeyState));*/

                        if (record.KeyEvent.wVirtualKeyCode == (int) ConsoleKey.Escape)
                        {
                            thread = null;
                        }
                    }
                    break;

                    case NativeMethods.StdInputHandle:
                    {
                        Console.WriteLine("Window buffer size event");
                        
                        
                    }break;
                }
                }
        }


        private class NativeMethods {

            public const Int32 StdInputHandle = -10;

            public const Int32 EnableMouseInput = 0x0010;
            public const Int32 EnableQuickEditMode = 0x0040;
            public const Int32 EnableExtendedFlags = 0x0080;

            public const Int32 KeyEvent = 1;
            public const Int32 MouseEvent = 2;


            [DebuggerDisplay("EventType: {" + nameof(EventType) + "}")]
            [StructLayout(LayoutKind.Explicit)]
            public struct InputRecord {
                [FieldOffset(0)]
                public Int16 EventType;
                [FieldOffset(4)] internal readonly KeyEventRecord KeyEvent;
                [FieldOffset(4)] internal readonly MouseEventRecord MouseEvent;
            }

            [DebuggerDisplay("{dwMousePosition.X}, {dwMousePosition.Y}")]
            public struct MouseEventRecord {
                public Coord dwMousePosition;
                public Int32 dwButtonState;
                public Int32 dwControlKeyState;
                public byte dwEventFlags;
            }

            [DebuggerDisplay("{X}, {Y}")]
            public struct Coord {
                public UInt16 X;
                public UInt16 Y;
            }

            [DebuggerDisplay("KeyCode: {" + nameof(wVirtualKeyCode) + "}")]
            [StructLayout(LayoutKind.Explicit)]
            public struct KeyEventRecord {
                [FieldOffset(0)]
                [MarshalAs(UnmanagedType.Bool)]
                public readonly Boolean bKeyDown;
                [FieldOffset(4)]
                public readonly UInt16 wRepeatCount;
                [FieldOffset(6)] internal UInt16 wVirtualKeyCode;
                [FieldOffset(8)]
                public readonly UInt16 wVirtualScanCode;
                [FieldOffset(10)]
                public readonly Char UnicodeChar;
                [FieldOffset(10)]
                public readonly Byte AsciiChar;
                [FieldOffset(12)]
                public readonly Int32 dwControlKeyState;
            };


            public class ConsoleHandle : SafeHandleMinusOneIsInvalid {
                public ConsoleHandle() : base(false) { }

                protected override bool ReleaseHandle() {
                    return true; //releasing console handle is not our business
                }
            }


            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern Boolean GetConsoleMode(ConsoleHandle hConsoleHandle, ref Int32 lpMode);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern ConsoleHandle GetStdHandle(Int32 nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern Boolean ReadConsoleInput(ConsoleHandle hConsoleInput, ref InputRecord lpBuffer, UInt32 nLength, ref UInt32 lpNumberOfEventsRead);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern Boolean SetConsoleMode(ConsoleHandle hConsoleHandle, Int32 dwMode);
        }

    }
}