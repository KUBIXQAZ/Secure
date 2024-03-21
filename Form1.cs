using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

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

        public string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public string myAppFolder;
        public string processesFilePath;

        public class ProcessToSecure
        {
            public string ProcessName;
            public string ProcessPath;
            public bool locked = true;
        }

        public Form1()
        {
            InitializeComponent();

            myAppFolder = Path.Combine(appDataPath, "KUBIXQAZ/Secure");
            processesFilePath = Path.Combine(myAppFolder, "processes.json");

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

            timer.Interval = 1;
            timer.Tick += timer_tick;
            timer.Start();

            LoadProcesses();
            processesToSecure = GetProcesses();
        }

        private string GetProcessNameFromFile(string filePath)
        {
            var process = Process.Start(filePath);
            string name = process.ProcessName;
            process.Kill();

            return name;
        }

        private void SaveProcesses()
        {
            string json = JsonConvert.SerializeObject(processesDirToSecure);

            if(!Directory.Exists(myAppFolder)) Directory.CreateDirectory(myAppFolder);

            File.WriteAllText(processesFilePath, json);
        }

        private void LoadProcesses()
        {
            if(File.Exists(processesFilePath))
            {
                string json = File.ReadAllText(processesFilePath);

                List<string> dirs = new List<string>();

                dirs = JsonConvert.DeserializeObject<List<string>>(json);

                processesDirToSecure = new List<string>(dirs);
            }
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

            foreach(string processDir in processesDirToSecure)
            {
                try
                {
                    processToSecure.Add(new ProcessToSecure
                    {
                        ProcessName = GetProcessNameFromFile(processDir),
                        ProcessPath = processDir
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
                        Write($"{CONSOLE_PREFIX} INPUT PASSWORD FOR '{processToSecure.ProcessName}':");
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

            string[] parts = input.Split(new char[] {' '}, 3, StringSplitOptions.RemoveEmptyEntries);


            if (parts[0] == EXIT_COMMAND)
            {
                if (user == admin)
                {
                    Application.Exit();
                } else
                {
                    Write($"{CONSOLE_PREFIX} ACCESS DENIED");
                }
            }
            else if (parts[0] == ADD_PROCESS_COMMAND)
            {
                if (user == admin)
                {
                    const string DISPLAY_PARAM = "-d";
                    const string ADD_PARAM = "-a";
                    const string REMOVE_PARAM = "-r";
                    const string SWITCH_LOCK_PARAM = "-s";

                    if (parts.Length == 2 && parts[1] == DISPLAY_PARAM)
                    {
                        if (processesToSecure.Count != 0)
                        {
                            int i = 0;
                            foreach (var process in processesToSecure)
                            {
                                Write($"{i}. LOCKED: [{(process.locked ? "ON" : "OFF")}] NAME: '{process.ProcessName}' DIR: '{process.ProcessPath}'");
                                i++;
                            }
                        }
                        else
                        {
                            Write($"{CONSOLE_PREFIX} NO DIRS IN PROCESSES DIR TO SECURE");
                        }
                    }
                    else if (parts.Length == 3)
                    {
                        if (parts[1] == ADD_PARAM)
                        {
                            string dir = parts[2].Trim('\'').Trim('"');
                            if (File.Exists(dir))
                            {
                                if (!processesDirToSecure.Contains(dir))
                                {
                                    Write($"{CONSOLE_PREFIX} ADDED NEW DIR TO PROCESSES DIR TO SECURE");
                                    processesDirToSecure.Add(dir);
                                    processesToSecure = GetProcesses();
                                    SaveProcesses();
                                }
                                else
                                {
                                    Write($"{CONSOLE_PREFIX} DIR '{dir}' ALREADY EXISTS IN PROCESSES DIR TO SECURE");
                                }
                            }
                            else
                            {
                                Write($"{CONSOLE_PREFIX} FILE DOES NOT EXIST");
                            }
                        }
                        else if (parts[1] == REMOVE_PARAM)
                        {
                            string param = parts[2];
                            if (int.TryParse(param, out int index))
                            {
                                if (index <= processesDirToSecure.Count - 1)
                                {
                                    Write($"{CONSOLE_PREFIX} REMOVED DIR '{processesDirToSecure[index]}' AT INDEX {index}");
                                    processesDirToSecure.RemoveAt(index);
                                    SaveProcesses();
                                }
                                else
                                {
                                    Write($"{CONSOLE_PREFIX} INDEX IS TO BIG");
                                }
                            }
                            else
                            {
                                string dir = param.Trim('\'').Trim('"');
                                if (processesDirToSecure.Contains(dir))
                                {
                                    processesDirToSecure.Remove(dir);
                                    Write($"{CONSOLE_PREFIX} REMOVED DIR '{dir}'");
                                }
                                else
                                {
                                    Write($"{CONSOLE_PREFIX} NO PROCESS WITH DIR '{dir}' FOUND");
                                }
                            }
                        }
                        else if (parts[1] == SWITCH_LOCK_PARAM)
                        {
                            string param = parts[2];
                            if (int.TryParse(param, out int index))
                            {
                                if (index <= processesDirToSecure.Count - 1)
                                {
                                    Write($"{CONSOLE_PREFIX} LOCK SWITCHED ON DIR '{processesDirToSecure[index]}' AT INDEX {index}");
                                    processesToSecure[index].locked = !processesToSecure[index].locked;
                                }
                                else
                                {
                                    Write($"{CONSOLE_PREFIX} INDEX IS TO BIG");
                                }
                            }
                            else
                            {
                                string dir = param.Trim('\'').Trim('"');
                                if (processesDirToSecure.Contains(dir))
                                {
                                    Write($"{CONSOLE_PREFIX} LOCK SWITCHED ON DIR '{dir}'");
                                    ProcessToSecure processToSwitch = null;
                                    foreach(var process in processesToSecure)
                                    {
                                        if (process.ProcessPath == dir) processToSwitch = process;
                                    }
                                    processesToSecure[processesToSecure.IndexOf(processToSwitch)].locked = !processesToSecure[processesToSecure.IndexOf(processToSwitch)].locked;
                                }
                                else
                                {
                                    Write($"{CONSOLE_PREFIX} NO PROCESS WITH DIR '{dir}' FOUND");
                                }
                            }
                        }
                        else
                        {
                            Write($"{CONSOLE_PREFIX} NO PARAM FOUND");
                        }
                    }
                    else
                    {
                        Write($"{CONSOLE_PREFIX} {ADD_PROCESS_COMMAND} {DISPLAY_PARAM} | {ADD_PARAM} '[DIR]' | {REMOVE_PARAM} '[DIR]'/[INDEX]");
                    }
                } else
                {
                    Write($"{CONSOLE_PREFIX} ACCESS DENIED");
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
                            Write($"{CONSOLE_PREFIX} LOGGED IN AS '{parts[1]}'");
                            user = _user;
                        }
                        else
                        {
                            Write($"{CONSOLE_PREFIX} SOMETHING WENT WRONG TRY AGAIN LATER");
                        }
                    }
                    else
                    {
                        Write($"{CONSOLE_PREFIX} THERE IS NO USER NAMED '{parts[1]}'");
                    }
                } else
                {
                    Write($"{CONSOLE_PREFIX} {CHANGE_USER_COMMAND} [NAME] [PASSWORD]");
                }
            }
            else if (parts[0] == MINIMIZE_COMMAND)
            {
                WindowState = FormWindowState.Minimized;
            }
            else if (parts[0] == USERS_COMMAND) {
                Write($"{CONSOLE_PREFIX} DISPLAYING USERS:");
                Write($"{CONSOLE_PREFIX} 'admin'");
                Write($"{CONSOLE_PREFIX} 'guest'");
            }
            else
            {
                Write($"{CONSOLE_PREFIX} UNRECOGNIZED COMMAND");
            }
        }

        public string EnterCommand()
        {
            string input = userInput.Text;
            userInput.Text = string.Empty;

            Write($"[{user.Name}]: {input}");

            if (awaiting)
            {
                List<ProcessToSecure> processes = new List<ProcessToSecure>(awaitingProcesses);
                foreach (var process in processes)
                {
                    if (admin.CheckPassword(input))
                    {
                        Write($"{CONSOLE_PREFIX} CORRECT PASSWORD");
                        processesToSecure[processesToSecure.IndexOf(process)].locked = false;
                        Process.Start(process.ProcessPath);
                    } else
                    {
                        Write($"{CONSOLE_PREFIX} INCORRECT PASSWORD");
                    }
                    awaitingProcesses.Remove(process);
                }

                if (awaitingProcesses.Count == 0)
                {
                    awaiting = false;
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