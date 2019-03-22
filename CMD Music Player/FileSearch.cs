using System;
using System.IO;
using System.Collections.Generic;

namespace CMD_Music_Player
{
    public static class FileSearch
    {
        public static List<string> ScanForMedia(string searchfolder, bool recursive = true, bool verbose = false)
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
        static bool IsSupportedFiletype(string filename)
        {
            //Simple function to check if a filename ends with an acceptable extension
            string[] supportedformats = new string[] { ".3g2", ".3gp", ".3gp2", ".3gpp", ".aac", ".adt", ".adts", ".aif", ".aifc", ".aiff", ".asf", ".asx", ".au", ".avi", ".cda", ".dvr-ms", ".flac", ".ivf", ".m1v", ".m2ts", ".m3u", ".m4a", ".m4v", ".mid", ".midi", ".mov", ".mp2", ".mp3", ".mp4", ".mp4v", ".mpa", ".mpe", ".mpeg", ".mpg", ".rmi", ".snd", ".wav", ".wax", ".wm", ".wma", ".wmd", ".wms", ".wmv", ".wmx", ".wmz", ".wpl", ".wvx" };
            foreach (string type in supportedformats)
            {
                if (filename.EndsWith(type)) return true;
            }
            return false;
        }
        public static bool IsElementInList(string SearchElement, List<string> ListToTest)
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
        public static string FindMediaPath(string SearchElement, List<string> ListToTest)
        {
            //Very similar to IsElementInList, but returns the filepath from the list.
            SearchElement = SearchElement.ToUpper();
            foreach (string element in ListToTest)
            {
                if (element.ToUpper().Contains(SearchElement)) return element;
            }
            throw new Exception("Unable to retrieve path from search term: No match found.");
        }
        public static (List<string>, List<string>) PerformScan()
        {
            //scan list of folders
            Console.WriteLine("Initialising registered folder index...");
            List<string> registeredfolders = new List<string> { }; //empty list to hold list of folders containing music
            foreach (string path in Properties.Settings.Default.RegisteredFolders) registeredfolders.Add(path); //reconstruct list of folder paths from stored paths

            //check for music in the application folder if enabled.
            if (Properties.Settings.Default.RegisterAppFolder) registeredfolders.Add(AppDomain.CurrentDomain.BaseDirectory);

            //Perform a recursive search on each folder for music files
            Console.WriteLine("Scanning registered folders...");
            List<string> discoveredfiles = new List<string> { };
            foreach (string folder in registeredfolders)
            {
                foreach (string file in FileSearch.ScanForMedia(folder))
                {
                    discoveredfiles.Add(file);
                }
            }
            Console.WriteLine(discoveredfiles.Count + " files found.");
            return (registeredfolders, discoveredfiles);
        }
    }
}