using System.Drawing;
using System.Net;
using WaviewPhotoSelector;

#region Variables d'affichage



string MAIN_MENU_STRING = 
    "[BEFORE SENDING] : Check link between RAW and JPG, rename files and prepare folders to send. " +
    "\n[BEFORE SENDING + UPLOAD] : Check link between RAW and JPG, rename files and prepare folders to send. Also upload thumbnails/photos to server." +
    "\n[FIND FILES MESSAGE] : Get all selected photos from a list of photos. Recieved by message" +
    "\n[FIND FILES SERVER] : Get all selected photos on the server"+
    "\n\nEnter your choice:\n\n-Before sending [b]\n-Before sending + upload [u]\n-Find files from message [f]\n-Find files from server [s]\n\nChoice [b / u / f / s]: ";

string END_STRING = 
    "DONE. Restart [r] or Quit [q] : ";

string WRONG_INPUT = 
    "Wrong input... Please try again : ";

string LIST_INPUT =
    "Please paste list of image numbers to find separated by ','. (ex : 1, 3, 6) : ";

string OUTPUT_FILE = "z_output.txt";
string URL = "http://www.waview.ch/akesso/gallery/";

#endregion

//WavFTP.UploadFile(null, "/test/test");

// Toutes les actions possibles par l'utilisateur.
// Ne pas oublier d'ajouter l'action dans la fonction "MakeAction" afin de pouvoir l'utiliser.
// Il faut également créer la méthode correspondante.
char[] CHOICES = { 'b', 'u', 'f', 's' };

do
{
    // Effacement de la console avant d'afficher le menu 
    Console.Clear();
    // Changement de la couleur de la console en blanc
    ChangeConsoleColor(ConsoleColor.White);
    // Affichage du message de menu principal
    Console.Write(MAIN_MENU_STRING);

    string choice = ReadLine().ToLower();

    // Tant que l'entrée de l'utilisateur n'est pas valable, on recommence a lui demander
    while (CheckInput(choice) == false)
    {
        ChangeConsoleColor(ConsoleColor.Red);
        Console.Write(WRONG_INPUT);
        ChangeConsoleColor(ConsoleColor.White);
        choice = ReadLine().ToLower();
    }
    
    MakeAction(choice);

    ChangeConsoleColor(ConsoleColor.Green);
    Console.Write(END_STRING);
    ChangeConsoleColor(ConsoleColor.White);
    
} while (ReadLine().ToLower() != "q");


/// <summary>
/// Permet de vérifier si l'entrée de l'utilisateur fait parti des choix possibles
/// </summary>
bool CheckInput(string input)
{
    if (input == "" || input == null)
    {
        return false;
    }
    
    return CHOICES.Contains(input.ToLower()[0]);
}

/// <summary>
/// Permet l'entrée d'un utilisateur dans la console
/// L'entrée sera alors retournée en minuscule
/// </summary>
string ReadLine()
{
    return Console.ReadLine();
}

/// <summary>
/// Effectuer l'action en fonction de l'entrée de l'utilisateur
/// </summary>
void MakeAction(string input)
{
    switch (input.ToLower())
    {
        case "b":
            B(false);
            break;
        case "f":
            FindFiles(null);
            break;
        case "s":
            S();
            break;
        case "u":
            B(true);
            break;
        default:
            break;
    }
}

void S()
{
    string choosen = GetPathOnServer();
    if (IsThereValidList(choosen))
    {
        ChangeConsoleColor(ConsoleColor.Green);
        Console.WriteLine("List found !");

        WebClient wc = new WebClient();
        string theTextFile = wc.DownloadString(URL + choosen + "/" + OUTPUT_FILE);

        //Console.WriteLine(theTextFile);
        GetFilesFromServer(theTextFile);
    }
    else
    {
        ChangeConsoleColor(ConsoleColor.Red);
        Console.WriteLine("List not found...");
    }
}

bool IsThereValidList(string path)
{
    List<string> files = WavFTP.listFiles("/"+path, false);
    files.Reverse();

    return files.Contains(OUTPUT_FILE);
}

string GetPathOnServer()
{
    int index = 1;    
    List<string> places = WavFTP.listFiles("", true);

    ChangeConsoleColor(ConsoleColor.Green);

    foreach (string place in places)
    {
        Console.WriteLine("["+index+"] " + place);
        index++;
    }

    // CHOIX DE L'ENDROIT
    bool ok = false;
    int choiceInt = -1;
    do
    {
        try
        {
            ChangeConsoleColor(ConsoleColor.White);
            Console.Write("Enter index of place to get photos from : ");
            string choice = ReadLine();
            choiceInt = int.Parse(choice);
            ok = true;

            if (choiceInt < 0 || choiceInt > places.Count)
            {
                ok = false;
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("Invalid index...");
            }

        }
        catch (Exception)
        {
            ChangeConsoleColor(ConsoleColor.Red);
            Console.WriteLine("Wrong input... Please try again");
            ok = false;
        }
    } while (!ok);

    string choosen_place = places[choiceInt-1];

    // CHOIX DE L'ALBUM PHOTO
    List<string> albums = WavFTP.listFiles("/"+choosen_place, true);
    index = 1;
    
    ChangeConsoleColor(ConsoleColor.Green);

    foreach (string album in albums)
    {
        Console.WriteLine("[" + index + "] " + album);
        index++;
    }

    ok = false;
    choiceInt = -1;
    do
    {
        try
        {
            ChangeConsoleColor(ConsoleColor.White);
            Console.Write("Enter index of the album to get photos from : ");
            string choice = ReadLine();
            choiceInt = int.Parse(choice);
            ok = true;

            if (choiceInt < 0 || choiceInt > places.Count)
            {
                ok = false;
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("Invalid index...");
            }

        }
        catch (Exception)
        {
            ChangeConsoleColor(ConsoleColor.Red);
            Console.WriteLine("Wrong input... Please try again");
            ok = false;
        }
    } while (!ok);

    string choosen_album = albums[choiceInt - 1];
    string path = choosen_place + "/" + choosen_album + "/";
    
    ChangeConsoleColor(ConsoleColor.Blue);
    Console.WriteLine("Choosen album : " + path);
    ChangeConsoleColor(ConsoleColor.White);
    return path;
}

void B(bool upload)
{
    string main_path = "";

    ChangeConsoleColor(ConsoleColor.White);
    // On demande à l'utilisateur de rentrer le chemin du dossier parent avec les photos
    Console.Write("Enter the path of the main cameras IMAGES folder : ");
    main_path = ReadLine();

    // On vérifie si le chemin existe
    if (Directory.Exists(main_path))
    {
        string[] directories = Directory.GetDirectories(main_path);
        string thumbnail_path = Path.Combine(main_path, "thumbnails");
        double ratio = 0;

        ChangeConsoleColor(ConsoleColor.Green);
        Console.WriteLine("Directory " + main_path + " is OK.");
        ChangeConsoleColor(ConsoleColor.White);

        Console.Write("Do you want to create thumbnails ? [y/n] : ");
        string choice = ReadLine().ToLower();

        if (choice == "y")
        {
            if (!Directory.Exists(thumbnail_path))
                Directory.CreateDirectory(thumbnail_path);
            
            if (thumbnail_path != null)
            {
                Console.Write("Enter the ratio of the thumbnails (0 to 1) : ");
                ratio = Convert.ToDouble(ReadLine());

                if (double.IsNaN(ratio))
                {
                    ChangeConsoleColor(ConsoleColor.Red);
                    Console.WriteLine("Wrong input... Ratio set to 0.2");
                    ChangeConsoleColor(ConsoleColor.White);
                    ratio = 0.2;
                }
                    
            }
        }

        Console.Clear();

        if (upload)
        {
            
        }

        string dir_if_needed = "";
        List<List<string>> all_files = new List<List<string>>();

        foreach (string directory  in directories)
        {
            if (directory.Contains("thumbnails"))
                break;

            string jpg_path = Path.Combine(main_path, directory, "JPG");
            string raw_path = Path.Combine(main_path, directory, "RAW");
            string main_camera_path = Path.Combine(main_path, directory);
            dir_if_needed = jpg_path;

            if (Directory.Exists(jpg_path) && Directory.Exists(raw_path))
            {
                if (choice == "y")
                    all_files.Add(BeforeSending(main_camera_path, jpg_path, raw_path, thumbnail_path, ratio, upload));
                else
                    all_files.Add(BeforeSending(main_camera_path, jpg_path, raw_path, null, 0, upload));
            }
            else
            {
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("Missing JPG or RAW folder in " + directory);
                ChangeConsoleColor(ConsoleColor.White);
            }
        }

        List<string> cover = new List<string>();
        if (choice == "y")
        {
            string path_to_file = Path.GetFullPath(Directory.GetFiles(thumbnail_path)[0]);
            string file_name = Path.GetFileName(path_to_file);
            File.Copy(path_to_file, path_to_file.Replace(file_name, "cover.JPG"), true);
            cover.Add(path_to_file.Replace(file_name, "cover.JPG"));
        }
        else
        {
            string path_to_file = Path.GetFullPath(Directory.GetFiles(dir_if_needed)[0]);
            string file_name = Path.GetFileName(path_to_file);
            File.Copy(path_to_file, Path.Combine(main_path, "cover.JPG"), true);
            cover.Add(Path.Combine(main_path, "cover.JPG"));
        }
        
        all_files.Add(cover);
        if (upload)
        {         
            WavFTP.UploadFile(all_files, CreateRemoteFolder());
            ChangeConsoleColor(ConsoleColor.Green);
            Console.WriteLine("Upload finished !");
            ChangeConsoleColor(ConsoleColor.White);
        }
        
    }
    else
    {
        ChangeConsoleColor(ConsoleColor.Red);
        Console.WriteLine("Path not found...");
        ChangeConsoleColor(ConsoleColor.White);
    }
}

/// <summary>
/// Permet de changer la couleur d'affichage de la console.
/// </summary>
void ChangeConsoleColor(ConsoleColor color)
{
    Console.ForegroundColor = color;
}

/// <summary>
/// Méthode permettant de vérifier si le lien entre le RAW et le JPG est bon
/// Cette méthode à pour objectif de vérifier la liaison entre les fichiers JPG et RAW.
/// Si les fichiers correspondent, alors on les renomme en fonction de la caméra. 
/// Sinon, les fichiers RAW sans JPG et les fichiers JPG sans fichiers RAW sont placés dans des dossier "poubelle"
/// </summary>
List<string> BeforeSending(string main_path, string jpg_path, string raw_path, string thumbnail_path, double ratio, bool upload)
{
    
    string jpg_trash_path = "NO_RAW_MATCH";
    string raw_trash_path = "NO_JPG_MATCH";
    int counter = 1;

    List<string> FilesToSend = new List<string>();


    // On vérifie si le chemin existe
    if (Directory.Exists(main_path))
    {
        string camera_name = Path.GetFileName(main_path);

        // On supprime le fichier DS STORE de tous les dossiers
        DeleteDSStore(main_path);
        DeleteDSStore(jpg_path);
        DeleteDSStore(raw_path);

        // On récupère les fichiers JPG et RAW
        string[] JPG_FILES = Directory.GetFiles(jpg_path);
        string[] RAW_FILES = Directory.GetFiles(raw_path);

        // Création des dossiers "poubelle"
        if (!Directory.Exists(Path.Combine(raw_path, raw_trash_path)))
            Directory.CreateDirectory(Path.Combine(raw_path, raw_trash_path));
        
        if (!Directory.Exists(Path.Combine(jpg_path, jpg_trash_path)))
            Directory.CreateDirectory(Path.Combine(jpg_path, jpg_trash_path));

        Console.WriteLine("Working on camera " + camera_name + "...");

        // On parcours toutes les images dans le dossier JPG
        foreach (string jpg_file in JPG_FILES)
        {
            //string[] photo_name_parts = Path.GetFileNameWithoutExtension(jpg_file).Split('_');
            //string code = photo_name_parts[photo_name_parts.Length - 1];
            string code = Path.GetFileNameWithoutExtension(jpg_file);

            // On cherche le fichier RAW correspondant
            string raw_file = RAW_FILES.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Equals(code), null);
            
            if (raw_file == null)
            {
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("NO RAW MATCH FOR : " + Path.GetFileName(jpg_file));
                ChangeConsoleColor(ConsoleColor.White);
                
                // Si le fichier RAW n'existe pas, on le déplace dans le dossier "poubelle"
                File.Move(jpg_file, Path.Combine(jpg_path, jpg_trash_path, Path.GetFileName(jpg_file)));
            }
            else
            {
                // Si le fichier existe on renomme le raw et le jpg en fonction de la caméra
                string new_jpg_path = Path.Combine(jpg_path, camera_name + "_" + counter + Path.GetExtension(jpg_file));
                string new_raw_path = Path.Combine(raw_path, camera_name + "_" + counter + Path.GetExtension(raw_file));

                try
                {
                    File.Move(jpg_file, new_jpg_path, true);
                }
                catch (Exception)
                {
                    ChangeConsoleColor(ConsoleColor.Red);
                    Console.WriteLine("ERROR WITH JPG FILE : " + Path.GetFileName(jpg_file));
                    ChangeConsoleColor(ConsoleColor.White);
                }

                try
                {
                    File.Move(raw_file, new_raw_path, true);
                }
                catch (Exception)
                {
                    ChangeConsoleColor(ConsoleColor.Red);
                    Console.WriteLine("ERROR WITH RAW FILE : " + Path.GetFileName(raw_file));
                    ChangeConsoleColor(ConsoleColor.White);
                }
                
                

                if (thumbnail_path != null)
                {
                    FilesToSend.Add(CreateThumbnail(new_jpg_path, thumbnail_path, ratio, null));
                }
                else
                {
                    FilesToSend.Add(new_jpg_path);
                }
                // On incrémente le compteur
                counter++;
            }
        }

        // On parcours toutes les images dans le dossier RAW
        foreach (string raw_file in RAW_FILES)
        {
            string[] photo_name_parts = Path.GetFileNameWithoutExtension(raw_file).Split('_');
            string code = photo_name_parts[photo_name_parts.Length - 1];

            // On cherche le fichier JPG correspondant
            string jpg_file = JPG_FILES.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Contains(code));

            // Si le fichier JPG n'existe pas, on le déplace dans le dossier "poubelle"
            if (jpg_file == null)
            {
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("NO JPG MATCH FOR : " + Path.GetFileName(raw_file));
                ChangeConsoleColor(ConsoleColor.White);

                // Si le fichier RAW n'existe pas, on le déplace dans le dossier "poubelle"
                File.Move(raw_file, Path.Combine(raw_path, raw_trash_path, Path.GetFileName(raw_file)));
            }
        }

        ChangeConsoleColor(ConsoleColor.Cyan);
        Console.WriteLine("Camera " + camera_name + " : " + counter + " photos" + " --> DONE.");
        ChangeConsoleColor(ConsoleColor.White);
    }
    else
    {
        ChangeConsoleColor(ConsoleColor.Red);
        Console.WriteLine("The path doesn't exist. Please try again. Press ENTER");
        Console.ReadKey();
    }

    return FilesToSend;
}

string CreateRemoteFolder()
{
    int index = 1;
    List<string> places = WavFTP.listFiles("", true);

    ChangeConsoleColor(ConsoleColor.Green);

    foreach (string place in places)
    {
        Console.WriteLine("[" + index + "] " + place);
        index++;
    }

    Console.WriteLine("[" + index + "] New folder");

    // CHOIX DE L'ENDROIT
    bool ok = false;
    int choiceInt = -1;
    do
    {
        try
        {
            ChangeConsoleColor(ConsoleColor.White);
            Console.Write("Enter index of place to get photos from : ");
            string choice = ReadLine();
            choiceInt = int.Parse(choice);
            ok = true;

            if (choiceInt < 0 || choiceInt > places.Count+1)
            {
                ok = false;
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("Invalid index...");
            }

        }
        catch (Exception)
        {
            ChangeConsoleColor(ConsoleColor.Red);
            Console.WriteLine("Wrong input... Please try again");
            ok = false;
        }
    } while (!ok);

    string placeName = "";
    
    if (choiceInt > places.Count)
    {
        Console.Write("Enter name of new folder : ");
        placeName = ReadLine();

        Console.Write("Enter the name of the place : ");
        string place = ReadLine();

        string fileName = @"C:\Users\Constantin\Documents\folder_name.txt";

        // Check if file already exists. If yes, delete it.     
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        // Create a new file     
        using (StreamWriter sw = File.CreateText(fileName))
        {
            sw.WriteLine(place);
        }

        string f = WavFTP.CreateFolder(placeName);
        WavFTP.UploadFile(fileName, f, "folder_name.txt");

    }
    else
    {
        placeName = places[choiceInt - 1];
    }
    

    Console.Write("Enter name of the folder to create : ");
    string folderName = ReadLine();

    string path = WavFTP.CreateFolder(placeName + "/" + folderName);
    string folder = WavFTP.CreateFolder(placeName);

    if (choiceInt > places.Count)
    {
        Console.Write("Path to place cover : ");
        string cover = ReadLine();
        WavFTP.UploadFile(cover, folder, "cover.jpg");
    }

    return placeName + "/" + folderName;
}

/// <summary>
/// Create a thumbnail image in the thumbnail folder of a % of the size of the original image
/// </summary>
string CreateThumbnail(string photo_path, string thumbnail_path, double ratio, string new_name)
{
    if (File.Exists(photo_path))
    {
        if (Directory.Exists(thumbnail_path))
        {
            string path = Path.Combine(thumbnail_path, (new_name != null ? new_name : Path.GetFileName(photo_path)));
            ResizeImageOnRatio(Image.FromFile(photo_path), ratio).Save(path);
            return path;
        }
    }

    return null;
}

Bitmap ResizeImageOnRatio(Image source_image, double ratio)
{
    if (ratio < 0 || ratio > 1)
        ratio = 0.5;

    int new_width = (int)(source_image.Width * ratio);
    int new_height = (int)(source_image.Height * ratio);

    Bitmap resized_image = new Bitmap(new_width, new_height);
    using (Graphics g = Graphics.FromImage(resized_image))
    {
        g.DrawImage(source_image, 0, 0, new_width, new_height);
    }

    return resized_image;
}

/// <summary>
/// On parcours le dossier raw à la recherche de toutes les images dans la liste
/// </summary>
void FindFiles(string list)
{
    string main_path = "";
    string raw_path = "";
    
    string selected_path = "";
    string not_selected_path = "";
    
    Console.Clear();

    ChangeConsoleColor(ConsoleColor.White);
    // On demande à l'utilisateur de rentrer le chemin du dossier parent avec les photos
    Console.Write("Enter the path of the main folder of the camera : ");
    main_path = ReadLine();

    raw_path = Path.Combine(main_path, "RAW");
    if (!Directory.Exists(raw_path))
    {
        ChangeConsoleColor(ConsoleColor.Red);
        Console.WriteLine("The path RAW doesn't exist. No folders found.");
        return;
    }

    // On vérifie si le chemin existe
    if (Directory.Exists(raw_path))
    {

        selected_path = Path.Combine(main_path, "SELECTED");
        if (!Directory.Exists(selected_path))
            Directory.CreateDirectory(selected_path);

        // On supprime le fichier DS STORE de tous les dossiers
        DeleteDSStore(main_path);
        DeleteDSStore(raw_path);

        Console.Write(LIST_INPUT);
        string index_list;

        if (list == null)
        {
            index_list = ReadLine();
        }
        else
        {
            index_list = list;
        }

        string[] splitted_indexes = index_list.Split(',');
        List<string> indexes = new List<string>();

        // Création d'une liste avec les index
        foreach (string item in splitted_indexes)
        {
            if (item.Trim() != "" && !indexes.Contains(item.Trim()))
                indexes.Add(item.Trim());
        }

        string[] RAW_FILES = Directory.GetFiles(raw_path);
        

        int counter = 0;
        foreach (string index in indexes)
        {
            bool match = false;
            foreach (string raw in RAW_FILES)
            {
                string[] photo_name_parts = Path.GetFileNameWithoutExtension(raw).Split('_');
                string code = photo_name_parts[photo_name_parts.Length - 1];
                
                // Si le code de l'image correspond à l'index
                if (code == index)
                {
                    ChangeConsoleColor(ConsoleColor.Green);
                    Console.WriteLine("FOUND : " + Path.GetFileName(raw));
                    ChangeConsoleColor(ConsoleColor.White);
                    File.Move(raw, Path.Combine(selected_path, Path.GetFileName(raw)));
                    counter++;
                    match = true;
                    break;
                }
            }

            // Si on ne trouve pas de correspondance, on affiche un message d'erreur
            if (match == false)
            {
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("NOT FOUND : " + index);
                ChangeConsoleColor(ConsoleColor.White);
            }
        }

        // On affiche le nombre de photos trouvées
        ChangeConsoleColor(ConsoleColor.Yellow);
        Console.WriteLine("Found " + counter + "/" + indexes.Count + " files.");

        // On déplace les fichiers non sélectionnés dans le dossier "NOT_SELECTED"
        ChangeConsoleColor(ConsoleColor.Gray);
        Console.WriteLine("Moving all other files into another folder ...");

        RAW_FILES = Directory.GetFiles(raw_path);
        not_selected_path = Path.Combine(raw_path, "NOT_SELECTED");
        if (!Directory.Exists(not_selected_path))
            Directory.CreateDirectory(not_selected_path);

        foreach (string raw_file in RAW_FILES)
        {
            File.Move(raw_file, Path.Combine(not_selected_path, Path.GetFileName(raw_file)));
        }
    }
    else
    {
        ChangeConsoleColor(ConsoleColor.Red);
        Console.WriteLine("The path doesn't exist. Please try again. Press ENTER");
        Console.ReadKey();
        // On relance la méthode
        FindFiles(null);
    }
}

void GetFilesFromServer(string myList)
{
    string main_path = "";
    string selected_path = "";
    string selected_name = "SELECTED";
    int cpt = 0;

    string[] list = myList.Split(",");
    List<string> indexes = new List<string>();

    foreach (string l in list)
        indexes.Add(l.Split(".")[0]);

    Console.Clear();

    ChangeConsoleColor(ConsoleColor.White);
    // On demande à l'utilisateur de rentrer le chemin du dossier parent avec les photos
    Console.Write("Enter the path of the main folder (images) : ");
    main_path = ReadLine();

    selected_path = Path.Combine(main_path, selected_name);

    if (!Directory.Exists(selected_path))
        Directory.CreateDirectory(selected_path);

    string[] directories = Directory.GetDirectories(main_path);

    ChangeConsoleColor(ConsoleColor.Yellow);
    foreach (string dir in directories)
    {
        if (dir.Contains("thumbnails") || dir.Contains(selected_name))
            break;

        string raw_path = Path.Combine(dir, "RAW");
        if (Directory.Exists(raw_path))
        {
            string[] files = Directory.GetFiles(raw_path);
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                foreach (string index in indexes)
                {
                    if (fileName == index)
                    {
                        Console.WriteLine("MATCH : " + fileName);
                        File.Copy(file, Path.Combine(selected_path, Path.GetFileName(file)));
                        cpt++;
                    }
                }
            }
        }
    }
    ChangeConsoleColor(ConsoleColor.Green);
    Console.WriteLine("Found : " + cpt + "/" + indexes.Count);
    ChangeConsoleColor(ConsoleColor.White);
}

/// <summary>
/// Permet de supprimer le fichier DS STORE d'un dossier
/// </summary>
void DeleteDSStore(string path)
{
    string[] files = Directory.GetFiles(path);
    foreach (string file in files)
    {
        if (Path.GetFileName(file).Contains(".DS_Store"))
            File.Delete(file);
    }
}

