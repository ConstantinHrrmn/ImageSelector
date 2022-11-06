#region Variables d'affichage

string MAIN_MENU_STRING = 
    "[BEFORE SENDING] : Check link between RAW and JPG, rename files and prepare folders to send. " +
    "\n[FIND FILES] : Get all selected photos from a list of photos. " +
    "\nEnter your choice. Before sending [b] or Find files [f] : ";

string END_STRING = 
    "DONE. Restart [r] or Quit [q] : ";

string WRONG_INPUT = 
    "Wrong input... Please try again : ";

string LIST_INPUT =
    "Please paste list of image numbers to find separated by ','. (ex : 1, 3, 6) : ";

#endregion

// Toutes les actions possibles par l'utilisateur.
// Ne pas oublier d'ajouter l'action dans la fonction "MakeAction" afin de pouvoir l'utiliser.
// Il faut également créer la méthode correspondante.
char[] CHOICES = { 'b', 'f' };

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
            BeforeSending();
            break;
        case "f":
            FindFiles();
            break;
        default:
            break;
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
void BeforeSending()
{
    string main_path = "";
    string jpg_path = "";
    string raw_path = "";
    
    string jpg_trash_path = "NO_RAW_MATCH";
    string raw_trash_path = "NO_JPG_MATCH";
    

    int counter = 1;

    Console.Clear();

    ChangeConsoleColor(ConsoleColor.White);
    // On demande à l'utilisateur de rentrer le chemin du dossier parent avec les photos
    Console.Write("Enter the path of the main folder : ");
    main_path = ReadLine();

    // On vérifie si le chemin existe
    if (Directory.Exists(main_path))
    {
        string camera_name = Path.GetFileName(main_path);
        
        // On vérifie si le dossier JPG existe
        jpg_path = Path.Combine(main_path, "JPG");
        if (!Directory.Exists(jpg_path))
        {
            do
            {
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("The path JPG doesn't exist. Please try again.");

                ChangeConsoleColor(ConsoleColor.White);
                Console.Write("Enter the path of the JPG folder : ");
                jpg_path = ReadLine();

            } while (!Directory.Exists(jpg_path));
        }

        // On vérifie si le dossier RAW existe
        raw_path = Path.Combine(main_path, "RAW");
        if (!Directory.Exists(raw_path))
        {
            do
            {
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("The path RAW doesn't exist. Please try again.");

                ChangeConsoleColor(ConsoleColor.White);
                Console.Write("Enter the path of the RAW folder : ");
                raw_path = ReadLine();

            } while (!Directory.Exists(raw_path));
        }

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

        // On parcours toutes les images dans le dossier JPG
        foreach (string jpg_file in JPG_FILES)
        {
            string[] photo_name_parts = Path.GetFileNameWithoutExtension(jpg_file).Split('_');
            string code = photo_name_parts[photo_name_parts.Length - 1];

            // On cherche le fichier RAW correspondant
            string raw_file = RAW_FILES.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Contains(code));
            
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
                File.Move(jpg_file, Path.Combine(jpg_path, camera_name + "_" + counter + Path.GetExtension(jpg_file)));
                File.Move(raw_file, Path.Combine(raw_path, camera_name + "_" + counter + Path.GetExtension(raw_file)));
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
    }
    else
    {
        ChangeConsoleColor(ConsoleColor.Red);
        Console.WriteLine("The path doesn't exist. Please try again. Press ENTER");
        Console.ReadKey();
        // On relance la méthode
        BeforeSending();
    }
}

/// <summary>
/// On parcours le dossier raw à la recherche de toutes les images dans la liste
/// </summary>
void FindFiles()
{
    string main_path = "";
    string raw_path = "";
    string selected_path = "";
    string not_selected_path = "";
    
    Console.Clear();

    ChangeConsoleColor(ConsoleColor.White);
    // On demande à l'utilisateur de rentrer le chemin du dossier parent avec les photos
    Console.Write("Enter the path of the main folder : ");
    main_path = ReadLine();

    // On vérifie si le chemin existe
    if (Directory.Exists(main_path))
    {
        // On vérifie si le dossier RAW existe
        raw_path = Path.Combine(main_path, "RAW");
        if (!Directory.Exists(raw_path))
        {
            do
            {
                ChangeConsoleColor(ConsoleColor.Red);
                Console.WriteLine("The path RAW doesn't exist. Please try again.");

                ChangeConsoleColor(ConsoleColor.White);
                Console.Write("Enter the path of the RAW folder : ");
                raw_path = ReadLine();

            } while (!Directory.Exists(raw_path));
        }

        selected_path = Path.Combine(raw_path, "SELECTED");
        if (!Directory.Exists(selected_path))
            Directory.CreateDirectory(selected_path);

        // On supprime le fichier DS STORE de tous les dossiers
        DeleteDSStore(main_path);
        DeleteDSStore(raw_path);

        Console.Write(LIST_INPUT);
        string index_list = ReadLine();

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
        FindFiles();
    }
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