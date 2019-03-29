using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CMD_Music_Player
{
    public class FileSearch
    {
        private static HelperFunctions functions = new HelperFunctions();

        public List<string> ScanForMedia(string searchfolder, bool recursive = true, bool verbose = false)
        {
            List<string> found_files = new List<string> { }; //list to store discovered files.

            //check if we have permission to access the folder
            try { Directory.GetFiles(searchfolder); }
            catch { return new List<string> { }; }

            //add all the files in the folder that are music files.
            foreach (string filepath in Directory.GetFiles(searchfolder))
            {
                if (IsSupportedFiletype(filepath))
                {
                    if (verbose) Console.WriteLine("Discovered " + filepath);
                    found_files.Add(filepath);
                }
            }

            //recursively check all subdirectories if flag was set
            if (recursive)
            {
                foreach (string folderpath in Directory.GetDirectories(searchfolder))
                {
                    if (verbose) Console.WriteLine("Traversing " + folderpath);
                    List<string> sub_files = ScanForMedia(folderpath);
                    foreach (string file in sub_files)
                    {
                        if (IsSupportedFiletype(file)) found_files.Add(file);

                    }
                }
            }

            //return the list of discovered files
            if (verbose) Console.WriteLine(found_files.Count + " files found.");
            return found_files;
        }
        bool IsSupportedFiletype(string filename)
        {
            //Simple function to check if a filename ends with an acceptable extension
            string[] supportedformats = new string[] { ".3g2", ".3gp", ".3gp2", ".3gpp", ".aac", ".adt", ".adts", ".aif", ".aifc", ".aiff", ".asf", ".asx", ".au", ".avi", ".cda", ".dvr-ms", ".flac", ".ivf", ".m1v", ".m2ts", ".m3u", ".m4a", ".m4v", ".mid", ".midi", ".mov", ".mp2", ".mp3", ".mp4", ".mp4v", ".mpa", ".mpe", ".mpeg", ".mpg", ".rmi", ".snd", ".wav", ".wax", ".wm", ".wma", ".wmd", ".wms", ".wmv", ".wmx", ".wmz", ".wpl", ".wvx" };
            foreach (string type in supportedformats)
            {
                if (filename.EndsWith(type)) return true;
            }
            return false;
        }
        public bool IsElementInList(string SearchElement, StringCollection ListToTest)
        {
            if (Core.bypassfilecheck) return true; //check for override

            //Simple function to check if a test string appears inside a list of strings
            SearchElement = SearchElement.ToUpper();
            foreach (string element in ListToTest)
            {
                if (element.ToUpper().Contains(SearchElement)) return true;
            }
            return false;
        }
        public string FindMediaPath(string SearchElement, StringCollection ListToTest)
        {
            //Very similar to IsElementInList, but returns the filepath from the list.
            SearchElement = SearchElement.ToUpper();
            foreach (string element in ListToTest)
            {
                if (element.ToUpper().Contains(SearchElement)) return element;
            }
            throw new Exception("Unable to retrieve path from search term: No match found.");
        }
        public StringCollection PerformScan()
        {
            //scan list of folders
            Console.WriteLine("Initialising registered folder index...");
            StringCollection registeredfolders = new StringCollection(); //empty list to hold list of folders containing music

            //reconstruct list of folder paths from stored paths
            if (Properties.Settings.Default.RegisteredFolders.Count > 0) //are there any registered folders? (this check may throw NullReference if empty, hence the 'try')
            {
                //if so, add them!
                foreach (string path in Properties.Settings.Default.RegisteredFolders)
                {
                    //check if the folder path is still valid before adding it.
                    if (Directory.Exists(path)) registeredfolders.Add(path);
                    else functions.Warning("'" + path + "' no longer exists. Ignoring...");
                }
            }


            //check for music in the application folder if enabled.
            if (Properties.Settings.Default.RegisterAppFolder) registeredfolders.Add(AppDomain.CurrentDomain.BaseDirectory);

            //Perform a recursive search on each folder for music files
            Console.WriteLine("Scanning registered folders...");
            StringCollection discoveredfiles = new StringCollection();
            foreach (string folder in registeredfolders)
            {
                foreach (string file in ScanForMedia(folder))
                {
                    //only add files that don't already exist
                    if (!discoveredfiles.Contains(file)) discoveredfiles.Add(file);
                }
            }
            Console.WriteLine(discoveredfiles.Count + " files found in " + registeredfolders.Count + " folders.");
            return discoveredfiles;
        }
    }
}