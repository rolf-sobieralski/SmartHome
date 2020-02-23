using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NICE_LOGGER;

//SMARTHOMEDB Klasse zur Abfrage der zugrundeliegenden SQLite Datenbank

namespace dirc1000
{
    class SMARTHOMEDB
    {
        SQLiteConnection verbindung = new SQLiteConnection();
        NiceLogger NL;
        public SMARTHOMEDB(NiceLogger N)//Initialisierung der Datenbankklasse
        {
            NL = N;//den Logger in dieser Klasse verfügbar machen
        }
        void WriteLog(LogType LType, string message)//funktion für den selbst geschriebenen Logger, um Log dateien im Programm Startverzeichniss abzulegen.
        {
            NL.WriteToLog(LType, message);
        }
        public bool CreateDatabase()//Datenbank erstellen falls nicht vorhanden
        {
            if (!File.Exists(Application.StartupPath + "\\dircdb.sqlite"))//check ob Datenbank nich nicht existiert
            {
                try
                {
                    SQLiteConnection.CreateFile(Application.StartupPath + "\\dircdb.sqlite");//Versuchen eine SQLite Datenbank für Clients, Commands und Ebenen zu erzeugen
                    create_tables();//Aufruf zum erstellen der Tabellen
                    return true;//true zurückgeben
                }catch(Exception e)
                {
                    WriteLog(LogType.Error,"Datenbank konnte nicht erstellt werden" + e.ToString());//Wenn Fehler auftaucht, diesen ins Log schreiben
                    return false;//false zurückgeben
                }
            }
            else
            {
                return true;//Wenn Datenbank bereits vorhanden ist, true zurückgeben
            }
        }
        void create_tables()//Alle Tabellen für die neue Datenbank anlegen
        {
            try
            {
                if (verbindung.State == ConnectionState.Open)//Check ob Verbindung bereits offen ist
                {
                    string sql = "create table SmartHomeClients (id int, IP_ADRESSE varchar(15), ClientName varchar(20),ebene int, button_x int, button_y int)";//SQL Statement zum  erstellen der ersten Tabelle
                    SQLiteCommand command = new SQLiteCommand(sql, verbindung);//Command erstellen um Tabelle anzulegen
                    command.ExecuteNonQuery();//Command ausführen
                    sql = "create table Commands (IP varchar(20), CommandName varchar(20), Command varchar(30))";//Statement für zweite Tabelle
                    command = new SQLiteCommand(sql, verbindung);//neuen Command erstellen
                    command.ExecuteNonQuery();//nochmal Command ausführen
                    sql = "create table Bereich (ID int, Name varchar(20), imageName varchar(30))";//Drittes Statement für die letzte Tabelle
                    command = new SQLiteCommand(sql, verbindung);//neuer Command
                    command.ExecuteNonQuery();//Letzte Command ausführung
                }
                else
                {
                    OpenConnection(Application.StartupPath + "\\dircdb.sqlite");//wenn die Verbindung nochnicht offen ist, erstmal eine Verbindung zur Datenbank öffnen
                    string sql = "create table SmartHomeClients (id int, IP_ADRESSE varchar(15), ClientName varchar(20),ebene int, button_x int, button_y int)";//erstes Statement für die Tabelle
                    SQLiteCommand command = new SQLiteCommand(sql, verbindung);//Neuer Command zum ausführen der Anweisung
                    command.ExecuteNonQuery();//Anweisung zum Command ausführen
                    sql = "create table Commands (IP varchar(20), CommandName varchar(20), Command varchar(30))";//zweites Statement für Tabelle
                    command = new SQLiteCommand(sql, verbindung);//zweiter neuer Command 
                    command.ExecuteNonQuery();//zweite Anweisung ausführen
                    sql = "create table Bereich (ID int, Name varchar(20), imageName varchar(30))";//letztes Statement zum Anlegen der letzten Tabelle
                    command = new SQLiteCommand(sql, verbindung);//letzter Command
                    command.ExecuteNonQuery();//Anweisung zum ausführen des letzten Command
                }
            }catch(Exception e)
            {
                WriteLog(LogType.Error, "Tabellen erstellung fehlgeschlagen\n" + e.ToString());//Log schreiben wenn irgendein Fehler autritt
            }
        }
        public List<SmartHomeBereich> get_bereiche()//Bereiche aus Datenbank holen und an aufrufenden Prozess übergeben
        {
            List<SmartHomeBereich> Bereiche = new List<SmartHomeBereich>();// neue Liste für Bereiche erstellen
            try
            {

                string CommandText = "SELECT * FROM Bereich;";//Statement um Daten aus der Smart Home Ebenen Tabelle zu bekommen
                SQLiteCommand command = new SQLiteCommand(CommandText, verbindung);//neuer Command um Abfrage durchzuführen
                SQLiteDataReader dr = command.ExecuteReader();//Command ausführen und ergebenisse speichern

                    while (dr.Read())//Schleife zum verarbeiten der Daten
                    {
                    string imageFilePath = Application.StartupPath + "\\Bereiche\\" + dr["imageName"].ToString();//Datei Pfad für angegebenes HG Bild
                    string bereichsName = dr["Name"].ToString();//Name des Bereiches
                    int BereichsID = Convert.ToInt16(dr["ID"].ToString());//ID des Bereiches
                        Image HG_Bild = Image.FromFile(imageFilePath);//eigendliches Hintergrund Bild erstellen
                        SmartHomeBereich bereich = new SmartHomeBereich(HG_Bild, dr["Name"].ToString(), Convert.ToInt32(dr["ID"].ToString()), dr["imageName"].ToString());//neuen Bereich anhand der erhaltenen Daten und des erzeugten Bildes erstellen
                        Bereiche.Add(bereich);//Bereich der Liste hinzufügen
                    }

            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim holen der Tabellennamen " + ex.ToString());//Log schreiben falls Fehler auftritt
                return null;//null zurückgeben
            }
            return Bereiche;//gefundene Bereiche zurückgeben
        }
        public void OpenConnection(string database)//Datenbank Verbindung öffnen
        {
            try
            {
                verbindung.ConnectionString = "Data Source=" + database;//Verbindungs String erstellen
                verbindung.Open();//Verbindung zu angegebener Datenquelle öffnen
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler bei Verbindungsherstellung" + ex.ToString());//Log schreiben falls Fehler auftritt
            }

        }
        public void close()// Verbindung schließen
        {
            try
            {
                verbindung.Close();//Verbindung zur Datenbank schließen
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim trennen der Verbindung" + ex.ToString());//Log schreiben falls Fehler auftritt
            }
        }
        public void update_data(string IP, string oldCommand,string newCommand, string oldValue, string newValue)//Commands aktualisieren
        {
            try
            {
                    string sql = "UPDATE Commands SET CommandName = '" + newCommand + "', Command = '" + newValue + "' WHERE IP = '" + IP + "' AND CommandName = '" + oldCommand + "'";//Statement zum aktualisieren eines bereits vorhandenen Commands
                    SQLiteCommand command = new SQLiteCommand(sql, verbindung);//neuen Command erstellen
                    command.ExecuteNonQuery();//Command ausführen
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim Update der Daten" + ex.ToString());//Log schreiben falls Fehler auftritt
            }

        }
        public void update_data(string IP, string newName)//Smart Home Clients aktualisieren
        {
            try
            {
                string sql = "UPDATE SmartHomeClients SET ClientName = '" + newName + "' WHERE IP_ADRESSE = '" + IP + "'";//Statement zum aktualisieren des bereits vorhandenen Smart Home Clients
                SQLiteCommand command = new SQLiteCommand(sql, verbindung);//neuer Command um Client zu aktualisieren
                command.ExecuteNonQuery();//Command ausführen
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim Update der Daten" + ex.ToString());//Log schreiben falls Fehler auftritt
            }

        }
        public bool client_exists(string IP)//Abfrage ob angegebener Client bereits in der Datenbank vorhanden ist
        {
            bool check_client = false;//Rückgabe Variable
            try
            {

                string CommandText = "SELECT * FROM SmartHomeClients WHERE IP_ADRESSE='" + IP + "'";//Statement zur Abfrage nach vorhandenem Client anhand der IP
                SQLiteCommand command = new SQLiteCommand(CommandText, verbindung);//neuen Command für die Datenbank Abfrage erstellen
                SQLiteDataReader dr = command.ExecuteReader();//Command ausführen
                while (dr.Read())//Schleife zum verarbeiten der Einträge
                {

                    if(dr["IP_ADRESSE"].ToString() == IP)//check ob aktueller Eintrag mit der übergebenen IP übereinstimmt
                    {
                        check_client = true;//Rückgabewert auf True setzen
                    }
                }
                return check_client;//Variable zurückgeben

            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim holen der Tabellennamen " + ex.ToString());//Log schreiben falls fehler auftritt
                return false;//false zurückgeben
            }
        }
        public void remove_client_button(string IP)//Button in Datenbank zu angegebenem Client zurücksetzen
        {
            string sql = "UPDATE SmartHomeClients SET button_x = 0, button_y = 0, ebene = 0 WHERE IP_ADRESSE = '" + IP + "'";//Statement um den bereits vorhandenen Button eines Client zurück zu setzen(x und y werden auf 0 gesetzt)
            SQLiteCommand command = new SQLiteCommand(sql, verbindung);//neuen Commmand für die Abfrage erstellen
            command.ExecuteNonQuery();//Command ausfüren
        }
        public void update_client(string IP,int EbenenID, int posX, int posY)//Smart Home Client aktualisieren
        {
            try
            {
                string sql = "UPDATE SmartHomeClients SET button_x = " + posX + ", button_y = " + posY + ", ebene = " + EbenenID.ToString() + " WHERE IP_ADRESSE = '" + IP + "'";//Statement zum aktualisieren des angegebenen Clients
                SQLiteCommand command = new SQLiteCommand(sql, verbindung);//neuen Command für die Abfrage erstellen
                command.ExecuteNonQuery();//Command ausführen
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim Update der Daten" + ex.ToString());//Log schreiben falls Fehler auftritt
            }
        }
        public void update_Bereich(int ID, string DateiName)//Smart Home Bereich aktualisieren
        {
            try
            {
                string sql = "UPDATE Bereich SET imageName = '" + DateiName + "' WHERE ID = " + ID.ToString();//Statement zum aktualisieren des angegebenen Bereichs
                SQLiteCommand command = new SQLiteCommand(sql, verbindung);//neuen Command für die Abfrrage erstellen
                command.ExecuteNonQuery();//Command ausführen
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim Update der Daten" + ex.ToString());//Log schreiben falls Fehler auftritt
            }
        }
        public void insert_Bereich(int BereichsID, string BereichsName, string BildName)//neuen Smart Home Bereich hinzufügen
        {
            string sql = "INSERT INTO Bereich (ID , Name , imageName) values(" + BereichsID + ",'" + BereichsName + "','" + BildName + "')";//Statement zum erstellen eines neuen Smart Home bereiches
            SQLiteCommand command = new SQLiteCommand(sql, verbindung);//neuen Command zur Datenbank Abfrage erstellen
            command.ExecuteNonQuery();//Command ausführen
        }
        public void insert_Client(int id, string ip, string name,int ebene, int button_x, int button_y)//neuen Smart Home Client hinzufügen
        {
            string sql = "insert into SmartHomeClients (id, IP_ADRESSE, ClientName, ebene, button_x, button_y) values (" + id.ToString() + ", '" + ip + "', '" + name + "', " + ebene + ", " + button_x.ToString() + ", " + button_y.ToString() +")";//Statement zum anlegen eines neuen Smart Home Clients
            SQLiteCommand command = new SQLiteCommand(sql, verbindung);//neuen Command zur Datenbank Abfrage erstellen
            command.ExecuteNonQuery();//Command ausführen
        }
        public void insert_command(string IP, string commandtext, string comm)//neuen Command hinzufügen
        {
            string sql = "insert into Commands (IP, CommandName, Command) values ('" + IP + "', '" + commandtext + "', '" + comm + "')";//Statement zum anlegen eines neuen Command für den angegebenen Client
            SQLiteCommand command = new SQLiteCommand(sql, verbindung);//neuen Command zur Datenbank Abfrage erstellen
            command.ExecuteNonQuery();//Command ausführen
        }
        public void delete_command(string IP, string commandText)//Command löschen
        {
            string CommandText = "DELETE from Commands WHERE IP = '" + IP + "' AND CommandName = '" + commandText + "'";//Statement zum löschen des angegebenen Command
            SQLiteCommand command = new SQLiteCommand(CommandText, verbindung);//neuen Command für die Datenbank Abfrage erstellen
            command.ExecuteNonQuery();//Command ausführen
        }
        public List<string> fill_client_data(string ip)//Smart Home Client Infos holen
        {
            List<string> client_infos = new List<string>();//neue Liste für Client Infos
            try
            {
                
                string CommandText = "SELECT * FROM SmartHomeClients WHERE IP_ADRESSE='" + ip + "'";//Statement um die Infos für den angegebenen Client zu holen
                SQLiteCommand command = new SQLiteCommand(CommandText, verbindung);//neuen Command für die Datenbank Abfrage ersteellen
                SQLiteDataReader dr = command.ExecuteReader();//Command ausführen und Daten zur verarbeitung speichern
                while (dr.Read())//Schleife zum lesen der erhaltenen Daten
                {
                    client_infos.Add(dr["id"].ToString());//ID in die Liste eintragen
                    client_infos.Add(dr["IP_ADRESSE"].ToString());//IP in die Liste eintragen
                    client_infos.Add(dr["ClientName"].ToString());//Name in die Liste eintragen
                }
                
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim holen der Tabellennamen " + ex.ToString());//Log schreiben falls ein Fehler auftritt
                return null;//null zurückgeben
            }
            return client_infos;//infos zum Client zurückgeben
        }
        public List<List<string>> fill_client_commands(string IP)//Commands zum angegebenen Client holen und zurückgeben
        {
            List<List<string>> client_Command_infos = new List<List<string>>();//Liste für Commands erstellen
            try
            {

                string CommandText = "SELECT * FROM Commands WHERE IP='" + IP + "'";//Statement zum holen der Commands zum angegebenen Client
                SQLiteCommand command = new SQLiteCommand(CommandText, verbindung);//neuen Command für die Datenbank Abfrage erstellen
                SQLiteDataReader dr = command.ExecuteReader();//Command ausführen und Daten zur verarbeitung speichern
                while (dr.Read())//Schleife zum verarbeiten der Daten
                {
                    List<string> command_reihe = new List<string>();//neue Liste for einen Command in der Command Liste
                    
                    command_reihe.Add(dr["CommandName"].ToString());//Command Name in die command_reihe schreiben
                    command_reihe.Add(dr["Command"].ToString());//Command in die command_reihe schreiben
                    client_Command_infos.Add(command_reihe);//command_reihe in client_Command_infos schreiben
                }

            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler beim holen der Tabellennamen " + ex.ToString());//Log schreiben falls Fehler auftritt
                return null;//null zurückgeben
            }
            return client_Command_infos;//Liste mit Commands zurückgeben
        }       
        public List<List<string>> get_clients()//Liste aller vorhandenen Clients holen und zurückgeben
        {

            List<List<string>> result_list = new List<List<string>>();//neue Liste für die vorhandenen Clients
            try
            {
                string CommandText = "SELECT * FROM SmartHomeClients";//Statement zur Abfrage aller vorhandenen Clients in der Datenbank
                SQLiteCommand command = new SQLiteCommand(CommandText, verbindung);//neuen Command zur Datenbank Abfrage erstellen
                    SQLiteDataReader dr = command.ExecuteReader();//Command ausführen und ergebnis zur verarbeitung speichern
                    while (dr.Read())//Schleife zum verarbeiten der Ergebnisse
                    {
                        List<string> result_row = new List<string>();//neue Liste für einen Client aus der Datenbank
                        result_row.Add(dr["id"].ToString());//ID hinzufügen
                        result_row.Add(dr["IP_ADRESSE"].ToString());//IP Adresse hinzufügen
                        result_row.Add(dr["ClientName"].ToString());//Name hinzufügen
                        result_row.Add(dr["ebene"].ToString());//Ebenen ID hinzufügen
                        result_row.Add(dr["button_x"].ToString());//X Coord des Button hinzufügen(kann 0 seien, wenn Client keinen Button hat)
                        result_row.Add(dr["button_y"].ToString());//Y Coord des Button hinzufügen(kann 0 seien, wenn Client keinen Button hat)
                        result_list.Add(result_row);//Client zur Rückgabeliste hinzufügen
                    }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.Error, "Fehler bei der Suche" + ex.ToString());//Log schreiben falls Fehler auftritt
                return null;//null zurückgeben
            }
            return result_list;//Clients zurückgeben
        }
    }
}
