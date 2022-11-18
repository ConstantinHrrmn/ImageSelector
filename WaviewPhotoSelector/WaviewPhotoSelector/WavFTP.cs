using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace WaviewPhotoSelector
{
    class WavFTP
    {
        static string host = @"ssh.waview.ch";
        static string username = "waview.ch";
        static string password = @"ssh_waview_!";
        static string remoteDirectory = "/customers/3/8/4/waview.ch/httpd.www/akesso/gallery/";

        public static List<string> listFiles(string path, bool directory)
        {
            List<string> folders = new List<string>();
            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();


                    var files = sftp.ListDirectory(remoteDirectory + path);

                    foreach (var file in files)
                    {
                        if (file.Name != "." && file.Name != "..")
                        {
                            if (directory)
                            {
                                if (file.IsDirectory)
                                {
                                    folders.Add(file.Name);
                                }
                            }
                            else
                            {
                                folders.Add(file.Name);
                            }
                        }

                    }

                    sftp.Disconnect();
                    return folders;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has been caught " + e.ToString());
                    return null;
                }
            }
        }

        public static void UploadFile(List<List<string>> all_files, string path)
        {
            if (!path.Contains(path))
                path = remoteDirectory + path;

            Console.WriteLine(path);

            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();
                    
                    sftp.ChangeDirectory(CreateFolder(path));

                    foreach (List<string> item in all_files)
                    {
                        foreach (string i in item)
                        {
                            Console.WriteLine(i);
                            using (FileStream fs = new FileStream(i, FileMode.Open))

                            {
                                sftp.BufferSize = 4 * 1024;

                                sftp.UploadFile(fs, Path.GetFileName(i));
                            }
                        }
                    }

                    sftp.Disconnect();

                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has been caught " + e.ToString());
                }

            }

        }

        public static void UploadFile(string file, string path, string name)
        {
            Console.WriteLine("Path for cover : " + path);
            if (!path.Contains(path))
                path = remoteDirectory + path;

            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();
                    
                    sftp.ChangeDirectory(path);

                    using (FileStream fs = new FileStream(file, FileMode.Open))

                    {
                        sftp.BufferSize = 4 * 1024;

                        sftp.UploadFile(fs, name);
                    }

                    sftp.Disconnect();
                    Console.WriteLine("Upload complete.");

                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has been caught " + e.ToString());
                }
            }
        }

        public static string CreateFolder(string path)
        {
            path = remoteDirectory + path;

            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();

                    string current = "";

                    if (path[0] == '/')
                    {
                        path = path.Substring(1);
                    }

                    while (!string.IsNullOrEmpty(path))
                    {
                        int p = path.IndexOf('/');
                        current += '/';
                        
                        if (p >= 0)
                        {
                            current += path.Substring(0, p);
                            path = path.Substring(p + 1);
                        }
                        else
                        {
                            current += path;
                            path = "";
                        }

                        try
                        {
                            SftpFileAttributes attrs = sftp.GetAttributes(current);
                            if (!attrs.IsDirectory)
                            {
                                throw new Exception("not directory");
                            }
                        }
                        catch (SftpPathNotFoundException)
                        {
                            sftp.CreateDirectory(current);
                        }
                    }

                    sftp.Disconnect();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(current);
                    Console.ForegroundColor = ConsoleColor.White;
                    return current;

                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has been caught " + e.ToString());
                }

                return null;
            }
        }
    }
}
