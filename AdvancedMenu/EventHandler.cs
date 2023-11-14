using System;
using System.Collections.Generic;

namespace MenuSystem
{
    public static class EventHandler
    {
        private static bool _isWaitingForInput = false;
        private static List<Action<int, int, int, int>> _mouseHandlers = new List<Action<int, int, int, int>>();
        private static List<Action<char, int>> _keyboardHandlers = new List<Action<char, int>>();
        private static bool _isWaitingForCustomInput = false;
        private static Action _customDraw = null;
        public static void MouseHandler(int x, int y, byte button, int buttonState)
        {
            if (Menu.actualMenu.IsOnMenu)
            {
                int i = Menu.actualMenu.GetMenuIndex(y, x);

                if (i != -1)
                {
                    Menu.actualMenu.SetField((short)i);
                    Menu.Draw();
                }
                
                if (button == 0 && buttonState==1)
                {
                    Menu.actualMenu.ExecuteAction();
                }
            }
            else if (!_isWaitingForCustomInput &&_isWaitingForInput && buttonState == 1)
            {
                _isWaitingForInput = false;
                Console.Clear();
                Menu.Draw();
            }
            else
            {
                for (var i = 0; i < _mouseHandlers.Count; i++)
                {
                    _mouseHandlers[i].Invoke(x,y,button,buttonState);
                }
            }
        }

        public static void KeyHandler(char c, ushort virtualkey)
        {
            //Console.WriteLine(c + "/"+ virtualkey);
            if (Menu.actualMenu.IsOnMenu)
            {
                if (virtualkey is >= 37 and <= 40 or 13)
                {
                    switch (virtualkey)
                    {
                        case 13:
                            Menu.actualMenu.ExecuteAction();    
                            break;
                    
                        case 38:
                            Menu.actualMenu.SetField((short)(Menu.actualMenu.GetField()-1));
                            Menu.Draw();
                            break;
                    
                        case 40:
                            Menu.actualMenu.SetField((short)(Menu.actualMenu.GetField()+1));
                            Menu.Draw();
                            break;
                    }
                    return;
                }
            
                for(int i = 0; i < Menu.actualMenu.Fields.Length; i++)
                {
                    string s = Menu.actualMenu.Fields[i];
                    if (s.Length == 0 || !string.Equals(s[0].ToString(), c.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    Menu.actualMenu.SetField((short)i);
                    Console.Clear();
                    Menu.Draw();
                }
            }
            else if (!_isWaitingForCustomInput && _isWaitingForInput)
            {
                _isWaitingForInput = false;
                Console.Clear();
                Menu.Draw();
            }
            else
            {
                for (var i = 0; i < _keyboardHandlers.Count; i++)
                {
                    _keyboardHandlers[i].Invoke(c, virtualkey);
                }
            }
        }

        public static void WaitForAction()
        {
            _isWaitingForInput = true;
        }
        
        public static void StopWaitForAction()
        {
            _isWaitingForInput = false;
        }
        
        public static void WaitForCustomAction()
        {
            _isWaitingForCustomInput = true;
        }
        
        public static void StopWaitingForCustomAction(bool waitForAction = true)
        {
            _isWaitingForCustomInput = false;
            if (waitForAction) Console.WriteLine("Appuyez sur une touche ou votre souris pour continuer");
            if (waitForAction) WaitForAction();
            else
            {
                Console.Clear();
                Menu.Draw();
            }
        }
        
        public static bool IsWaitingForCustomAction()
        {
            return _isWaitingForCustomInput;
        }
        
        public static void AddMouseHandler(Action<int, int, int, int> action)
        {
            _mouseHandlers.Add(action);
        }

        public static void RemoveMouseHandler(Action<int, int, int, int> action)
        {
            _mouseHandlers.Remove(action);
        }

        public static void AddKeyboardHandler(Action<char, int> action)
        {
            _keyboardHandlers.Add(action);
        }

        public static void RemoveKeyboardHandler(Action<char, int> action)
        {
            _keyboardHandlers.Remove(action);
        }
        public static Action GetCustomDraw()
        {
            return _customDraw;
        }
        public static void SetCustomDraw(Action action)
        {
            _customDraw = action;
        }
    }
}