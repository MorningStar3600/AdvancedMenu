using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace MenuSystem
{
    public class Menu
    {
        public static Menu actualMenu;

        private string[] _fields;
        private short _selectedField;
        private Action[] _actions;
        private Menu _parentMenu;
        private bool _isOnMenu;
        private bool _isCentered = true;
        private static string _projectCreator = "";
        private static List<Tuple<int, object>> _customData = new List<Tuple<int, object>>();

        public Menu(string[] fields, Action[] actions, bool setToScreen = true, Menu parent = null)
        {
            if (_projectCreator == "") throw new Exception("The project creator was not defined. Use 'Menu.SetCreator('your_creator_name');");
            ConsoleListener.Run();
            _fields = new string[fields.Length + 1];
            _actions = new Action[actions.Length + 1];
            for (int i = 0; i < fields.Length; i++)
            {
                _fields[i] = fields[i];
                _actions[i] = actions[i];
            }
            _fields[fields.Length] = parent == null ? "Quit" : "Back";
            _actions[actions.Length] = Quit;
            _selectedField = 0;
            _parentMenu = parent;
            
            if (setToScreen)
            {
                actualMenu = this;
                Console.Clear();
                Draw();
            }
        }

        public static void Draw()
        {
            Menu.actualMenu.IsOnMenu = true;
            
            if (_customData.Count > 0) Console.Clear();
            
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;
            
            if (height < actualMenu._fields.Length*2 + 1)
            {
                Console.WriteLine("The windows is too small to display the menu");
                return;
            }
            
            int nbrFields = actualMenu.Fields.Length;
            
            int actualHeight = height/2 - nbrFields;

            foreach (string field in Menu.actualMenu.Fields)
            {
                Console.SetCursorPosition(width/2 - field.Length/2, actualHeight);
                //Clear
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Black;
                for (int i = 0; i < field.Length+3; i++)
                {
                    Console.Write(" ");
                }
                Console.ResetColor();
                Console.SetCursorPosition(width/2 - field.Length/2, actualHeight);
                if (actualMenu.Field == field)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("-> " + field);
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("> "+field);
                }
                actualHeight+= 2;
            }

            if (_customData is null) return;
            foreach (var customData in _customData)
            {
                if (customData is null) continue;
                int pos = customData.Item1;
                object data = customData.Item2;
                if (data is null) continue;
                
                int x = width/2 - data.ToString()!.Length/2;
                int y;
                if (pos < 0)
                    y = height / 2 - nbrFields + pos;
                else y = actualHeight - 1 + pos;
                
                if (y < 0 || y > height) continue;
                Console.SetCursorPosition(x, y);
                Console.Write(data.ToString());
            }
        }
        
        public void SetToScreen()
        {
            actualMenu = this;
            Console.Clear();
            Draw();
        }

        public int GetMenuIndex(int y, int x)
        {
            int i = (y - Console.WindowHeight/2 - Fields.Length)/2 + Fields.Length;
            if (i < 0 || i >= Fields.Length) i = -1;
            
            if (i != -1 && (x < Console.WindowWidth/2 - Fields[i].Length/2 || x > Console.WindowWidth/2 + Fields[i].Length/2)) i = -1;
            return i;
        }

        public string Field
        {
            get => _fields[_selectedField];
        }
        
        public string[] Fields
        {
            get => _fields;
        }
        
        public Action Action
        {
            get => _actions[_selectedField];
        }
        
        public void SetField(short field)
        {
            if (field > _fields.Length - 1)
            {
                _selectedField = 0;
            }
            else if (field < 0)
            {
                _selectedField = (short) (_fields.Length - 1);
            }
            else
            {
                _selectedField = field;
            }
        }
        
        public short GetField()
        {
            return _selectedField;
        }

        public static void SetCreator(string name)
        {
            _projectCreator = name;
        }
        
        public void ExecuteAction()
        {
            Console.Clear();
            if (_actions[_selectedField] != null)
            {
                Menu.actualMenu.IsOnMenu = false;
                _actions[_selectedField].Invoke();
            }

            if (!EventHandler.IsWaitingForCustomAction() && !actualMenu.IsOnMenu)
            {
                Console.WriteLine("Press a key or your mouse to continue...");
                EventHandler.WaitForAction();
            }
            
        }

        public void AddCustomData(int pos, object data)
        {
            _customData.Add(new Tuple<int, object>(pos, data));
        }
        
        public void RemoveCustomData(int pos)
        {
            _customData.RemoveAt(pos);
        }

        private void Quit()
        {
            if (_parentMenu != null)
            {
                actualMenu = _parentMenu;
                Console.Clear();
                Draw();
            }
            else
            {
                EndCredits();
            }
            
        }

        public int GetFieldLength(int field)
        {
            return _fields[field].Length;
        }

        public bool IsOnMenu
        {
            get => _isOnMenu;
            set => _isOnMenu = value;
        }

        public void EndCredits()
        {
            int centerX = Console.WindowWidth/2;
            int centerY = Console.WindowHeight/2;
            
            string creator = "A project by " + _projectCreator;
            
            Console.SetCursorPosition(centerX - creator.Length/2, centerY-1);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            
            Console.Write("A project by ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(_projectCreator);
            
            Thread.Sleep(5000);
            
            Environment.Exit(0);
        }

    }
}