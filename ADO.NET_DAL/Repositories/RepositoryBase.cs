﻿using ADO.NET_DAL.Models;
using System.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System;

namespace ADO.NET_DAL.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private const string _CONNECTION_STRING = @"Server=WINDOWS-PU636AO;Database=PhoneBook;Trusted_Connection=True;";
        public bool Delete(int personId)
        {
            SqlConnection connection = new SqlConnection(_CONNECTION_STRING);
            SqlCommand command = new()
            {
                Connection = connection,
                CommandText = "DELETE FROM People WHERE  People.Id = @PeopleId",
            };
            command.Parameters.Add("@PeopleId", SqlDbType.Int).Value = personId;
            if (connection.State == ConnectionState.Closed) connection.Open();

            bool result = command.ExecuteNonQuery() >= 0;
            connection.Close();
            return result;
        }

        public List<Person> GetAllConnected()
        {
            SqlConnection connection = new SqlConnection(_CONNECTION_STRING);
            SqlCommand command = new()
            {
                Connection = connection,
                CommandText = "SELECT Id,FirstName,LastName,Phone,Email FROM People",
            };
            if (connection.State == ConnectionState.Closed) connection.Open();
            List<Person> list = new List<Person>();

            command.ExecuteNonQuery();
            //Data okuma 
            SqlDataReader dr = command.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    Person person = new()
                    {
                        Id = Convert.ToInt32(dr[0]),
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString(),
                        Phone = dr["Phone"].ToString(),
                        Email = dr["Email"].ToString()
                    };
                    list.Add(person);
                }
            }
            dr.Close();
            connection.Close();
            return list;
        }

        public List<Person> GetAllDisConnected()
        {
            string command = "SELECT Id,FirstName,LastName,Phone,Email FROM People";
            SqlDataAdapter da = new SqlDataAdapter(command, _CONNECTION_STRING);

            DataTable dt = new DataTable();
            da.Fill(dt);

            var list = (from DataRow dr in dt.Rows
                        select new Person
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            FirstName = dr["FirstName"].ToString(),
                            LastName = dr["LastName"].ToString(),
                            Phone = dr["Phone"].ToString(),
                            Email = dr["Email"].ToString()
                        }).ToList();


            return list;
        }

        public bool Insert(Person person)
        {
            SqlConnection connection = new SqlConnection(_CONNECTION_STRING);
            SqlCommand command = new()
            {
                Connection = connection,
                //CommandText = $"INSERT INTO People VALUES ('{person.FirstName}','{person.LastName}','{person.Phone}','{person.Email}')",
                CommandText = "INSERT INTO People VALUES (@FirstName, @LastName,@Phone , @Email)",
            };

            //command.Parameters.AddWithValue("@LastName",person.LastName) // istifade etmirik. Yavashdir deye
            command.Parameters.Add("@FirstName", SqlDbType.NVarChar).Value = person.FirstName;
            command.Parameters.Add("@LastName", SqlDbType.NVarChar).Value = person.LastName;
            command.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = person.Phone;
            command.Parameters.Add("@Email", SqlDbType.NVarChar).Value = person.Email;
            if (connection.State == ConnectionState.Closed) connection.Open();

            bool result = command.ExecuteNonQuery() > 0;
            connection.Close();
            return result;
        }

        public List<Person> Search(List<Expression<Func<T, bool>>> predicates)
        {

            foreach (var predicate in predicates)
            {
                if (predicate.Body is BinaryExpression body)
                {
                    SqlConnection connection = new SqlConnection(_CONNECTION_STRING);

                    string query = "SELECT * FROM People WHERE @Table = @Input";

                    if (body.Left is MemberExpression left)
                    {
                        query = query.Replace("@Table", left.Member.Name);
                    }
                    SqlCommand command = new()
                    {
                        Connection = connection,
                        CommandText = query,
                    };
                    if (body.Right is Expression right)
                    {
                        var lambdaExpression = Expression.Lambda(right);
                        var dele = lambdaExpression.Compile();
                        var table = $"{dele.DynamicInvoke()}";
                        command.Parameters.Add("@Input", SqlDbType.NVarChar).Value = table;
                    }

                    if (connection.State == ConnectionState.Closed) connection.Open();
                    List<Person> list = new List<Person>();
                    command.ExecuteNonQuery();
                    //Data okuma 
                    SqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            Person person = new()
                            {
                                Id = Convert.ToInt32(dr[0]),
                                FirstName = dr["FirstName"].ToString(),
                                LastName = dr["LastName"].ToString(),
                                Phone = dr["Phone"].ToString(),
                                Email = dr["Email"].ToString()
                            };
                            list.Add(person);
                        }
                    }
                    dr.Close();
                    connection.Close();
                    if (list.Count > 0) return list;
                    else continue;
                }

            }
                return null;
        }

        }
    }

