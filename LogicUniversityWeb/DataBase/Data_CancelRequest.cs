﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LogicUniversityWeb.DataBase
{
    public class Data_CancelRequest:DataLink
    {
      /*  public void DeleteRequestDetails(int RequestID)
        {

            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"UPDATE RequisitionList SET statusOfRequest='Cancelled' where RequisitionID = '" + RequestID + "'";          // Delete request details(items ) from Requsistion details tabel
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }

        }*/

        public void CancelRequest(int RequestID)
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"UPDATE RequisitionList SET statusOfRequest='Cancelled' where RequisitionID = '" + RequestID + "'";            // Update status of request to cancel with request number
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }

        }


        public void RejectRequest(int RequestID)
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"UPDATE RequisitionList SET statusOfRequest='Rejected' where RequisitionID = '" + RequestID + "'";            // Update status of request to cancel with request number
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }

        }
        public void RejectRequestwithComments(int RequestID, string comments)
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
               
                string cmdtext = @"UPDATE RequisitionList SET statusOfRequest='Rejected' and Comments ='" + comments + "'" + " where RequisitionID = '" + RequestID + "'";
                // Update status of request to cancel with request number
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }

        }
        public void ApproveRequest(int RequestID)
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"UPDATE RequisitionList SET statusOfRequest='Approved' where RequisitionID = '" + RequestID + "'";            // Update status of request to cancel with request number
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }

        }
        public void ApproveRequestwithComments(int RequestID, string Comments)
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"UPDATE RequisitionList SET statusOfRequest='Approved', Comments ='" + Comments + "'" + " where RequisitionID = '" + RequestID + "'";             // Update status of request to cancel with request number
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }

        }

       /* public void AddCommentsToReQuest(int RequestID, string Comments)
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
       
                string cmdtext = @"insert into RequisitionList (Comments) values ('" + Comments +"')"; //@"UPDATE RequisitionList SET statusOfRequest='Approved' and Comments ='" + Comments + "'" + " where RequisitionID = '" + RequestID + "'" ;            // Update status of request to cancel with request number


                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }

        }*/

    }
}