using System;
using System.IO;

/* syntax and basic usage for this library WAS googled,
   although some syntax was figured out through trial and error */
using Newtonsoft.Json.Linq;

namespace CMD_Music_Player
{
    //custom class I made to assist in accessing JSON database.
    public class DatabaseAccess
    {
        //variables to be access accross the class
        private JObject coredata;
        readonly private string filename;

        //executed on class initialisation
        public DatabaseAccess(string databaseFilename)
        {
            //set a variable to the specified database filename
            filename = databaseFilename;

            //read the file as string array and store as regular string
            string loadedJson = "";
            try { loadedJson = String.Join(" ", File.ReadAllLines(filename)); }
            catch (FileNotFoundException)
            {
                //recreate the database from a template if the file is missing
                Core.error("Database file missing; recreating...");
                File.WriteAllLines(filename, new string[]
                    {
                        "{",
                        "\t\"RegisteredFolders\": [],",
                        "\t\"RegisterAppFolder\": false",
                        "}"
                    }
                );
            }

            //interpret the database json as a modifiable object
            coredata = JObject.Parse(loadedJson); //https://www.newtonsoft.com/json/help/html/M_Newtonsoft_Json_Linq_JObject_Parse.htm
        }

        public string getAttribute(string attribute)
        {
            ///Gets the stored vavlue for an attribute
            return coredata[attribute].ToString();
        }
        public void setAttribute(string attribute, string new_value)
        {
            ///Sets the stored value for an attribute.
            coredata[attribute] = new_value; //update value
        }
        public void saveDatabase()
        {
            ///Writes the changes to the database to disk
            File.WriteAllLines(filename, new[]
            {
                coredata.Root.ToString()
            });
        }
    }
    public class GlobalDatabase { public static DatabaseAccess database; } //allows accessing the database object from anywhere in the code
}