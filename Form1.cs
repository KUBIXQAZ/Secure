using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Secure
{
    public partial class Form1 : Form
    {
        public Timer timer = new Timer();

        public bool awaiting = false;
        public List<ProcessToSecure> awaitingProcesses = new List<ProcessToSecure>();

        public User user;
        public User guest;
        public User admin;

        public List<string> processesDirToSecure = new List<string>();
        public List<ProcessToSecure> processesToSecure = new List<ProcessToSecure>();

        public const string CONSOLE_PREFIX = "[CONSOLE]:";

        public class User
        {
            public string Name;
            private string Password = string.Empty;

            public bool CheckPassword(string input)
            {
                if (input == Password) return true;
                else return false;
            }

            public void SetPassword(string password)
            {
                Password = password;
            }
        }

        public class ProcessToSecure
        {
            public string ProcessName;
            public string ProcessPath;
            public bool locked = true;
        }

        public Form1()
        {
            InitializeComponent();

            FormClosing += Closing;
            userInput.KeyDown += OnKeyDown;

            commandHistory.ReadOnly = true;

            admin = new User
            {
                Name = "admin"
            };
            admin.SetPassword("admin");

            guest = new User
            {
                Name = "guest"
            };

            user = guest;

            processesToSecure = GetProcesses();

            timer.Interval = 1;
            timer.Tick += timer_tick;
            timer.Start();
        }

        private string GetProcessNameFromFile(string filePath)
        {
            var process = Process.Start(filePath);
            string name = process.ProcessName;
            process.Kill();

            return name;
        }

        private void Write(string input)
        {
            commandHistory.Text += $"{input}\n";
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                EnterCommand();
            }
        }

        private void Closing(object s, FormClosingEventArgs e)
        {
            if(user != admin)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                }
            }
        }

        private List<ProcessToSecure> GetProcesses()
        {
            List<ProcessToSecure> processToSecure = new List<ProcessToSecure>();

            //test processes//
            processesDirToSecure.Add(@"D:\Windows\AppData\Roaming\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\password generator.lnk");
            processesDirToSecure.Add(@"D:\Windows\Desktop\AutoClicker.lnk");
            //end//

            foreach(string processDir in processesDirToSecure)
            {
                try
                {
                    processToSecure.Add(new ProcessToSecure
                    {
                        ProcessName = GetProcessNameFromFile(processDir)
                    });
                }
                catch { }
            }

            return processToSecure;   
        }

        private void timer_tick(object s, EventArgs e)
        {
            foreach(var process in processesToSecure)
            {
                if(process.locked)
                {
                    Process[] file = Process.GetProcessesByName(process.ProcessName);
                    if (file.Length != 0)
                    {
                        ProcessToSecure processToSecure = process;
                        process.ProcessPath = file[0].MainModule.FileName;
                        process.locked = true;

                        awaitingProcesses.Add(processToSecure);
                        awaiting = true;

                        Activate();
                        Write("[CONSOLE]: INPUT PASSWORD:");
                        file[0].Kill();
                    }

                    if (user != admin)
                    {
                        Process[] taskmgr = Process.GetProcessesByName("taskmgr");
                        if (taskmgr.Length != 0)
                        {
                            foreach (var task in taskmgr)
                            {
                                try
                                {
                                    task.Kill();
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        private void checkAdminCommands(string input)
        {
            const string CHANGE_USER_COMMAND = "user";
            const string USERS_COMMAND = "users";
            const string EXIT_COMMAND = "exit";
            const string MINIMIZE_COMMAND = "minimize";
            const string ADD_PROCESS_COMMAND = "process";

            string[] parts = input.Split(' ');

            if (parts[0] == EXIT_COMMAND)
            {
                Application.Exit();
            } else if (parts[0] == ADD_PROCESS_COMMAND)
            {
                const string DISPLAY_PARAM = "-d";
                const string ADD_PARAM = "-a";
                const string REMOVE_PARAM = "-r";

                if(parts.Length == 2)
                {
                    if (parts[1] == DISPLAY_PARAM)
                    {
                        if(processesDirToSecure.Count != 0)
                        {
                            int i = 0;
                            foreach (var dir in processesDirToSecure)
                            {
                                Write($"{i}. {dir}");
                                i++;
                            }
                        } else
                        {
                            Write("NO DIRS IN PROCESSES DIR TO SECURE");
                        }

                    }
                }
                else if (parts.Length == 3)
                {
                    if (parts[1] == ADD_PARAM)
                    {

                    }
                    else if (parts[1] == REMOVE_PARAM)
                    {

                    } 
                    else
                    {
                        Write($"{CONSOLE_PREFIX} NO PARAM FOUND");
                    }
                }
                else
                {
                    Write($"{CONSOLE_PREFIX} {ADD_PROCESS_COMMAND} {DISPLAY_PARAM} | {ADD_PARAM} | {REMOVE_PARAM} [DIR]");
                }
            }
            else if (parts[0] == CHANGE_USER_COMMAND)
            {
                if (parts.Length == 3)
                {
                    bool exist = false;
                    User _user = null;

                    if (parts[1] == guest.Name)
                    {
                        exist = true;
                        _user = guest;
                    } 
                    else if (parts[1] == admin.Name)
                    {
                        exist = true;
                        _user = admin;
                    }

                    if (exist)
                    {
                        if (_user.CheckPassword(parts[2]))
                        {
                            Write($"[CONSOLE]: LOGGED IN AS '{parts[1]}'");
                            user = _user;
                        } 
                        else
                        {
                            Write("[CONSOLE]: SOMETHING WENT WRONG TRY AGAIN LATER");
                        }
                    }
                    else
                    {
                        Write($"[CONSOLE]: THERE IS NO USER NAMED '{parts[1]}'");
                    }
                } else
                {
                    Write($"[CONSOLE]: {CHANGE_USER_COMMAND} [NAME] [PASSWORD]");
                }
            } 
            else if (parts[0] == MINIMIZE_COMMAND)
            {
                WindowState = FormWindowState.Minimized;
            } 
            else if (parts[0] == USERS_COMMAND) {
                Write("[CONSOLE]: DISPLAYING USERS:");
                Write($"[CONSOLE]: 'admin'");
                Write($"[CONSOLE]: 'guest'");
            } 
            else
            {
                Write("[CONSOLE]: UNRECOGNIZED COMMAND");
            }
        }

        public string EnterCommand()
        {
            string input = userInput.Text;
            userInput.Text = string.Empty;

            Write($"[{user.Name}]: {input}");

            if (awaiting)
            {
                if(awaitingProcesses.Count == 0)
                {
                    awaiting = false;
                }

                List<ProcessToSecure> processes = new List<ProcessToSecure>(awaitingProcesses);
                foreach (var process in processes)
                {
                    if (admin.CheckPassword(input))
                    {
                        Write("[CONSOLE]: CORRECT PASSWORD");
                        processesToSecure[processesToSecure.IndexOf(process)].locked = false;
                        Process.Start(process.ProcessPath);
                    } else
                    {
                        Write("[CONSOLE]: INCORRECT PASSWORD");
                    }
                    awaitingProcesses.Remove(process);
                }
            } 
            else
            {
                checkAdminCommands(input);
            }

            return input;
        }
    }
}