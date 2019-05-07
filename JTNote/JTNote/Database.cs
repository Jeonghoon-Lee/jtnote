﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    public class Database
    {
/*
        // Set DbConnectionString from DB Property
        const string DbConnectionString = @"Server=tcp:jtnote.database.windows.net,1433;Initial Catalog=JTNoteDB;Persist Security Info=False;User ID=sqladmin;Password=IPD16DotNet;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private SqlConnection conn;

        // DB Constructor
        public Database()
        {
            conn = new SqlConnection(DbConnectionString);
            conn.Open();
        }

        public List<User> GetAllUsers()
        {
            List<User> list = new List<User>();

            SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Users", conn);
            using (SqlDataReader reader = cmdSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = (int)reader["Id"];
                    string userName = (string)reader["UserName"];
                    string email = (string)reader["Email"];
                    string password = (string)reader["Password"];

                    list.Add(new User(id, userName, email, password));
                }
            }
            return list;
        }

        public bool ExistsEmail(string email)
        {
            SqlCommand cmdSelect = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@Email", conn);

            cmdSelect.Parameters.AddWithValue("Email", email);
            return (int)cmdSelect.ExecuteScalar() > 0;      // true if email exists in users table; otherwise, false
        }

        public int AddUser(User user)
        {
            SqlCommand cmdInsert = new SqlCommand("INSERT INTO Users (UserName, Email, Password) OUTPUT INSERTED.ID VALUES (@UserName, @Email, @Password)", conn);

            cmdInsert.Parameters.AddWithValue("UserName", user.UserName);
            cmdInsert.Parameters.AddWithValue("Email", user.Email);
            cmdInsert.Parameters.AddWithValue("Password", user.Password);

            user.Id = (int)cmdInsert.ExecuteScalar();
            return user.Id;   // return generated id
        }

        public User GetUser(string email)
        {
            User user = new User();

            SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Users WHERE Email=@Email", conn);
            cmdSelect.Parameters.AddWithValue("Email", email);

            using (SqlDataReader reader = cmdSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    user.Id = (int)reader["Id"];
                    user.UserName = (string)reader["UserName"];
                    user.Email = email;
                    user.Password = (string)reader["Password"];
                }
            }
            return user;
        }

        public void DeleteUserById()
        {
            // TODO: When user is deleted, we should delete all of the related notes, tags and notebooks.
        }

        public List<Note> GetAllNotesByUserId(int userId)
        {
            List<Note> returnList = new List<Note>();

            SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Notes WHERE UserId=@UserId", conn);
            cmdSelect.Parameters.AddWithValue("UserId", userId);
            using (SqlDataReader reader = cmdSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = (int)reader["Id"];
                    string title = (string)reader["Title"];
                    string content = (string)reader["Content"]; // TODO: Change to decode of XML blob when implementing formatting!
                    int? notebookId = reader["NotebookId"] as int?;
                    bool isDeleted = (byte)reader["IsDeleted"] == 1 ? true : false;                        
                    DateTime lastUpdatedDate = (DateTime)reader["LastUpdatedDate"];

                    returnList.Add(new Note(id, userId, title, content, notebookId, isDeleted, lastUpdatedDate));
                }
            }
            return returnList;
        }

        public Note GetNoteById(int noteId)
        {
            SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Notes WHERE UserId=@UserId AND Id=@NoteId", conn);
            cmdSelect.Parameters.AddWithValue("UserId", Globals.LoginUser.Id);
            cmdSelect.Parameters.AddWithValue("NoteId", noteId);

            using (SqlDataReader reader = cmdSelect.ExecuteReader())
            {
                int id = 0;
                string title = null, content = null;
                int? notebookId = null;
                bool isDeleted = false;
                DateTime lastUpdatedDate = DateTime.Now;

                while (reader.Read())
                {
                    id = (int)reader["Id"];
                    title = (string)reader["Title"];
                    content = (string)reader["Content"]; // TODO: Change to decode of XML blob when implementing formatting!
                    notebookId = reader["NotebookId"] as int?;
                    isDeleted = (byte)reader["IsDeleted"] == 1 ? true : false;
                    lastUpdatedDate = (DateTime)reader["LastUpdatedDate"];
                }

                return new Note(id, Globals.LoginUser.Id, title, content, notebookId, isDeleted, lastUpdatedDate);
            }
        }

        public string UpdateNote(Note inputNote)
        {
            SqlCommand cmdInsert = new SqlCommand("UPDATE Notes SET UserId=@UserId, Title=@Title, Content=@Content, NotebookId=@NotebookId, IsDeleted=@IsDeleted, LastUpdatedDate=@LastUpdatedDate WHERE Id=@NoteId", conn);

            cmdInsert.Parameters.AddWithValue("NoteId", inputNote.Id);
            cmdInsert.Parameters.AddWithValue("UserId", inputNote.UserId);
            cmdInsert.Parameters.AddWithValue("Title", inputNote.Title);
            cmdInsert.Parameters.AddWithValue("Content", inputNote.Content);
            cmdInsert.Parameters.AddWithValue("IsDeleted", inputNote.IsDeleted == true ? 1 : 0);
            cmdInsert.Parameters.AddWithValue("LastUpdatedDate", DateTime.Now);

            // Insert null into db if notebook ID is null, otherwise insert notebook ID
            if (inputNote.NotebookId == null)
                cmdInsert.Parameters.AddWithValue("NotebookId", DBNull.Value);
            else
                cmdInsert.Parameters.AddWithValue("NotebookId", inputNote.NotebookId);

            return (string)cmdInsert.ExecuteScalar();
        }

        public void DeleteNote(int id)
        {
            SqlCommand cmdDelete = new SqlCommand("DELETE FROM Notes WHERE Id=@NoteId", conn);
            cmdDelete.Parameters.AddWithValue("NoteId", id);
            cmdDelete.ExecuteNonQuery();
        }
*/
        public void CreateNote(Note inputNote)
        {
            SqlCommand cmdInsert = new SqlCommand("INSERT INTO Notes (UserId, Title, Content, NotebookId, IsDeleted, LastUpdatedDate) values (@UserId, @Title, @Content, @NotebookId, @IsDeleted, @LastUpdatedDate)", conn);

            cmdInsert.Parameters.AddWithValue("UserId", inputNote.UserId);
            cmdInsert.Parameters.AddWithValue("Title", inputNote.Title);
            cmdInsert.Parameters.AddWithValue("Content", inputNote.Content);
            cmdInsert.Parameters.AddWithValue("NotebookId", DBNull.Value); // To change if adding ability to add to notebook in note creation screen
            cmdInsert.Parameters.AddWithValue("IsDeleted", 0);
            cmdInsert.Parameters.AddWithValue("LastUpdatedDate", DateTime.Now);

            cmdInsert.ExecuteNonQuery();
        }
    }
}
