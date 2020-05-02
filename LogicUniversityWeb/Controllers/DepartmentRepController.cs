using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogicUniversityWeb.Models;
using LogicUniversityWeb.DataBase;
using System.Data.SqlClient;
using System.Diagnostics;
using LogicUniversityWeb.EmailAlerts;

namespace LogicUniversityWeb.Controllers
{
    [Authorize]
    public class DepartmentRepController : Controller
    {
        [Authorize(Roles = "DepRep,DepStaff,DepHead,InterimHead")]
        [Route("DepartmentRep/Home")]
        public ActionResult Home()
        {
            /* Users users = Data_Users.GetInfoByUserID((int)Session["UserID"]);
             ViewData["Userinfo"] = users;*/
            return View();
        }

        // GET: DepartmentRep
        [Authorize(Roles = "DepRep,DepStaff")]
        public ActionResult ViewCatalogue()
        {
            Data_Catalogue Dc = new Data_Catalogue();

            List<Stationary> Lt_Stationary = Dc.Stationery_List();

            ViewBag.prodList = Lt_Stationary;
            /*Users users = Data_Users.GetInfoByUserID((int)Session["UserID"]);

            ViewData["Userinfo"] = users;*/

            return View();
        }

        [HttpPost]
        public ActionResult Add2Cart(Stationary s)  //this method triggers when the "ADD" button is pressed for the item.
        {
            Debug.WriteLine(s.ItemID);

            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"insert into WishList (UserID,ItemID,ItemName,RequiredQuantity,UOM) values ('" + (int)Session["UserID"] + "','" + s.ItemID + "','" + s.ItemName + "','" + s.RequiredQuantity + "','" + s.UOM + "')";
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("ViewCatalogue", "DepartmentRep");
        }

        [Authorize(Roles = "DepRep,DepStaff")]
        public ActionResult RequisitionForm()    //Display requisition form
        {
            int ID = (int)Session["UserID"];

            Data_RequisitionForm Rc = new Data_RequisitionForm();

            List<WishList> Lt_wishlist = Rc.wishList(ID);

            ViewBag.wishList = Lt_wishlist;
            Users users = Data_Users.GetInfoByUserID((int)Session["UserID"]);

            ViewData["Userinfo"] = users;

            return View();
        }

        public ActionResult RemovefromCart(String Item_ID)                            //this method triggers when the "Remove" button is pressed for the item.
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"delete from WishList where UserID ='" + (int)Session["UserID"] + "' and ItemID='" + Item_ID + "'"; // + "and RequiredQuantity=" + Wt.RequiredQuantity;
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("RequisitionForm");
        }

        [HttpPost]
        public ActionResult SubmitRequisition(List<WishList> wishListofusers)                    //triggreed when Request is submitted
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"insert into RequisitionList (statusOfRequest, DateofSubmission , DeptID_FK, UserID_FK) values ('PendingforApproval'" + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + (String)Session["DeptID"] + "'," + (int)Session["UserID"] + ")";
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }
            int requestID = getRequisitionID();
            Debug.WriteLine(requestID);

            foreach (WishList wt in wishListofusers)
            {
                //int Temp =  wt.ItemID.Length;
                /*Debug.WriteLine(wt.ItemID);
                Debug.WriteLine(wt.ItemName);
                Debug.WriteLine(wt.UOM);
                Debug.WriteLine(wt.RequiredQuantity);*/

                using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
                {
                    conn.Open();
                    string cmdtext = @"insert into RequisitionDetail (RequisitionID,ItemID,RequisitionQuantity) values ('" + requestID + "','" + wt.ItemID + "','" + wt.RequiredQuantity + "')";
                    SqlCommand cmd = new SqlCommand(cmdtext, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            clearWishList((int)Session["UserID"]);

            //Email Alert to users fpr placing order.
            SendEmailNotification sen = new SendEmailNotification();
            Users userInfo = Data_Users.GetInfoByUserID((int)Session["UserID"]);
            Department department = Data_Department.GetDepartmentInfoByID(userInfo.DeptID_FK);
            String Useremail = userInfo.EmailID;
            String EmailSubject = "Request Submitted RequisitionID#" + requestID;
            String EmailBody = "<p> Dear " + userInfo.Username + ",</p>";
            EmailBody += "<p>Your order has been succesfully submitted here is the OrderID " + requestID + " for your reference. Currently, it is been pending for approval from your " + department.DepartmentHead + ".</br> We will notify you once it approved.</p>";
            EmailBody += "<p>Thank you<br/>Logic University Staionery Store</p>";
            EmailBody += "<p> Please do not reply to this email it is auto-generated.</p>";

            sen.SendEmailHTML(Useremail, EmailSubject, EmailBody);

            return RedirectToAction("MyRequisitions");
        }

        public int getRequisitionID()
        {
            RequisitionList reqInfo = new RequisitionList();

            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"SELECT TOP 1 RequisitionID from RequisitionList WHERE UserID_FK = '" + (int)Session["UserID"]
                    + "' AND DateofSubmission = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ORDER BY RequisitionID DESC";

                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    reqInfo = new RequisitionList();
                    reqInfo.RequisitionID = (int)reader["RequisitionID"];
                }
            }
            int RequetID = reqInfo.RequisitionID;
            return RequetID;
        }

        [HttpPost]
        public ActionResult SaveForm(List<WishList> wishListofusers)
        {

            int userID = (int)Session["UserID"];

            if (CountWishList(userID) != 0)          //To check wheather items are there are not in wishlist       
            {
                clearWishList(userID);
            }

            foreach (WishList wt in wishListofusers)
            {
                //int Temp =  wt.ItemID.Length;
                Debug.WriteLine(wt.ItemID);
                Debug.WriteLine(wt.ItemName);
                Debug.WriteLine(wt.UOM);
                Debug.WriteLine(wt.RequiredQuantity);

                using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
                {
                    conn.Open();
                    string cmdtext = @"insert into WishList (UserID,ItemID,ItemName,RequiredQuantity,UOM) values ('" + (int)Session["UserID"] + "','" + wt.ItemID + "','" + wt.ItemName + "','" + wt.RequiredQuantity + "','" + wt.UOM + "')";
                    SqlCommand cmd = new SqlCommand(cmdtext, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Home");
        }

        public void clearWishList(int userID)
        {
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"delete from WishList where UserID ='" + userID + "'";          // + "and RequiredQuantity=" + Wt.RequiredQuantity;
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }
        }

        public int CountWishList(int userID)                 //To check wheather items are there are not in wishlist                                   
        {
            using (SqlConnection C = new SqlConnection(DataLink.connectionString))
            {
                C.Open();

                string cmdtext = @"select count(*) as count from  WishList where UserID ='" + userID + "'";
                SqlCommand cmd = new SqlCommand(cmdtext, C);
                int count = (int)cmd.ExecuteScalar();
                return count;
            }
        }

        [Authorize(Roles = "DepRep,DepStaff")]
        [Route("DepartmentRep/MyRequisitions")]
        public ActionResult MyRequisitions()
        {
            int userID = (int)Session["UserID"];

            Data_MyRequisitions My = new Data_MyRequisitions();

            List<RequisitionList> requisitionLists = My.getRequisitionList(userID);

            ViewBag.ListofRequest = requisitionLists;
            Users users = Data_Users.GetInfoByUserID((int)Session["UserID"]);

            ViewData["Userinfo"] = users;

            return View();
        }

        [HttpPost]
        public ActionResult ViewRequestInfo(RequisitionList r)
        {
            Data_RequestDetails ReqDetails = new Data_RequestDetails();

            List<WishList> DetailsOfRequest = ReqDetails.RequestDetails(r.RequisitionID);

            ViewBag.DetailsOfRequest = DetailsOfRequest;

            ViewData["RequestID"] = r.RequisitionID;
            ViewData["Dateofsub"] = r.DateofSubmission;
            ViewData["status"] = r.statusOfRequest;
            Users users = Data_Users.GetInfoByUserID((int)Session["UserID"]);

            ViewData["Userinfo"] = users;

            return View();

        }

        [HttpPost]
        public ActionResult CancelOrder(int ID)
        {
            Data_CancelRequest DC = new Data_CancelRequest();
            DC.CancelRequest(ID);

            SendEmailNotification sen = new SendEmailNotification();
            Users userInfo = Data_Users.GetInfoByUserID((int)Session["UserID"]);
            Department department = Data_Department.GetDepartmentInfoByID(userInfo.DeptID_FK);
            String Useremail = userInfo.EmailID;
            String EmailSubject = "Cancellation of ReuestID#" + ID;
            String EmailBody = "<p> Dear " + userInfo.Username + ",</p>";
            EmailBody += "<p>We would like to inform you that your Request for stationery with RequisitionID " + ID + " has been cancelled by you .</p>";
            EmailBody += "<p>Thank you<br/>Logic University Staionery Store</p>";
            EmailBody += "<p> Please do not reply to this email it is auto-generated.</p>";

            sen.SendEmailHTML(Useremail, EmailSubject, EmailBody);
            return RedirectToAction("MyRequisitions");
        }

        [Authorize(Roles = "DepRep")]
        public ActionResult CollectionPoint()
        {
            Department deptInfo = Data_Department.FindUserDepartmentByUserId((string)Session["DeptID"]);

            ViewData["deptInfo"] = deptInfo;

            return View();
        }


        [HttpPost]
        public ActionResult UpdateCP(string DepartmentID, string Collectionpoint)
        {
            bool res = Data_Department.UpdateCollectionPoint(DepartmentID, Collectionpoint);


            SendEmailNotification sen = new SendEmailNotification();

            Users userInfo = Data_Users.GetInfoByUserID((int)Session["UserID"]);

            Department department = Data_Department.GetDepartmentInfoByID(userInfo.DeptID_FK);
            Users ClerkInfo =  Data_Users.GetStoreClerkInfo(userInfo.DeptID_FK);

            //String Useremail = userInfo.EmailID;
            //String EmailBody = "<p> Dear " + userInfo.Username + ",</p>";

            String EmailSubject = "Change Collection Point for " + department.Departmentname;

            //String EmailBody = "<p>Dear Yee Mon , </p>";
            String EmailBody = "<p> Dear " + ClerkInfo.Username + ",</p>";
            EmailBody += "<p>I am "+ userInfo.Username + " currently the  Department Representative for "+ department.Departmentname + ". We are pleased to say that Collection point for <b>  " + department.Departmentname + "</b> is amended . The updatest collection point is  <b>" + Collectionpoint + ".</p>";
            EmailBody += "<p>Thank you<br/>Logic University Staionery Store</p>";
            EmailBody += "<p> Please do not reply to this email it is auto-generated.</p>";

            sen.SendEmailHTML(ClerkInfo.EmailID, EmailSubject, EmailBody);
            
            return RedirectToAction("CollectionPoint");
        }

        
        [Authorize(Roles = "DepRep")]
        public ActionResult DisbursmentList()
        {

            string deptID = (string)Session["DeptID"];

            Data_DisbursementList DList = new Data_DisbursementList();

            List<DisbursementList> dlist = DList.FindDisbursementListByDepartmentID(deptID);
            ViewBag.ListofDisbursement = dlist;
            Users users = Data_Users.GetInfoByUserID((int)Session["UserID"]);

            ViewData["Userinfo"] = users;

            return View();

        }

        [HttpPost]
        public ActionResult ViewDisbursementDetail(DisbursementList dlist)
        {
            Data_DisbursementDetails details = new Data_DisbursementDetails();

            List<Stationary> DetailsOfDisbursement = details.DisbursementDetails(dlist.DisbursementID);

            ViewBag.DetailsOfDisbursement = DetailsOfDisbursement;

            ViewData["DisbursementID"] = dlist.DisbursementID;
            ViewData["DisbursementStatus"] = dlist.DisbursementStatus;

            Users users = Data_Users.GetInfoByUserID((int)Session["UserID"]);

            ViewData["Userinfo"] = users;

            return View();
        }

        [Authorize(Roles = "DepHead")]
        public ActionResult ViewDelegations()
        {
            Data_Delegation data_Delegation = new Data_Delegation();
            List<Delegations> delegationslist = data_Delegation.Delegation_List((string)Session["DeptID"]);

            ViewBag.delegationslist = delegationslist;
            ViewBag.DeptID = (string)Session["DeptID"];

            return View();
        }

        [HttpPost]
        public ActionResult RemoveDelegation(Delegations Dg)  //this method triggers when the "ADD" button is pressed for the item.
        {
            int IsUnAllocated;
            //Debug.WriteLine(s.ItemID);
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"UPDATE Delegation SET DelegationStatus ='UnAllocated' where DelegationID = '" + Dg.DelegationID + "'";
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                IsUnAllocated = cmd.ExecuteNonQuery();
            }

            if(IsUnAllocated !=0)
            {
                using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
                {
                    conn.Open();
                    string cmdtext = @"UPDATE Users SET role ='DepStaff' where Username = '" + Dg.Username + "'";
                    SqlCommand cmd = new SqlCommand(cmdtext, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("ViewDelegations");
        }

        [HttpPost]
        public ActionResult DelegateEmployee(string DeptID)
        {
            List<Users> DeptEmployees = Data_Users.GetAllDeptStaffByDeptID((string)Session["DeptID"]);

            SelectList list = new SelectList(DeptEmployees, "UserID", "Username");

            ViewBag.DepartmentEmps = list;

            return View();
        }

        [HttpPost]
        public ActionResult SaveDelegation(Delegations d)
        {
            int IsAllocated;
            Users user = Data_Users.GetInfoByUserID(d.UserID);
            using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
            {
                conn.Open();
                string cmdtext = @"insert into Delegation (DeptID,UserID,StartDate,EndDate,Username,DelegationStatus) values ('" + user.DeptID_FK + "','" + user.UserID + "','" + d.StartDate + "','" + d.EndDate + "','" + user.Username + "','Allocated')";
                SqlCommand cmd = new SqlCommand(cmdtext, conn);
                 IsAllocated = cmd.ExecuteNonQuery();
            }

            if(IsAllocated!=0)
            {
                using (SqlConnection conn = new SqlConnection(DataLink.connectionString))
                {
                    conn.Open();
                    string cmdtext = @"UPDATE Users SET role ='InterimHead' where UserID = '" + d.UserID + "'";
                    SqlCommand cmd = new SqlCommand(cmdtext, conn);
                    IsAllocated = cmd.ExecuteNonQuery();
                }
            }
            return Redirect("ViewDelegations");
        }

        [Authorize(Roles = "DepHead,InterimHead")]
        [Route("DepartmentRep/ApproveRequisition")]
        public ActionResult ApproveRequisition()
        {
            Data_ApproveRequisition data_ApproveRequisition = new Data_ApproveRequisition();
            List<RequisitionList> Lt_Requisitions = data_ApproveRequisition.getRequisitionListForApproval((string)Session["DeptID"]);

            ViewBag.RequestList = Lt_Requisitions;

            return View();
        }

        [HttpPost]
        public ActionResult ApproveRequisitionDetails(RequisitionList r)
        {
            Data_RequestDetails ReqDetails = new Data_RequestDetails();

            List<WishList> DetailsOfRequest = ReqDetails.RequestDetails(r.RequisitionID);

            ViewBag.DetailsOfRequest = DetailsOfRequest;

            ViewData["RequestID"] = r.RequisitionID;
            ViewData["Dateofsub"] = r.DateofSubmission;
            ViewData["status"] = r.statusOfRequest;

            return View();
        }

        [HttpPost]
        public ActionResult RejectRequest(int ID, string Comments)
        {
            Data_CancelRequest DC = new Data_CancelRequest();

            if (Comments.Length == 0) { DC.RejectRequest(ID); }

            else { DC.RejectRequestwithComments(ID, Comments);  }


            //return RedirectToAction("ApproveRequisition");

            //return RedirectToAction("ApproveRequisition", "DepartmentRep");
            return Redirect("~/DepartmentRep/ApproveRequisition");

        }

        [HttpPost]
        public ActionResult ApproveRequest(int ID ,string Comments)
        {
            Data_CancelRequest DC = new Data_CancelRequest();

            if (Comments.Length == 0) 
            { 
                DC.ApproveRequest(ID); 
            }
            else
            {
                //DC.ApproveRequest(ID);
                 DC.ApproveRequestwithComments(ID, Comments);
            }
           
            //return RedirectToAction("ApproveRequisition");
            return Redirect("~/DepartmentRep/ApproveRequisition");
        }

        [Authorize(Roles = "DepHead,InterimHead")]
        public ActionResult Assginrepresentative()
        {
           Users DepRepInfo  = Data_Users.GetDepRepInfo((string)Session["DeptID"]);

            ViewBag.DepRepName = DepRepInfo.Username;

              List<Users> DeptSatff  = Data_Users.GetAllDeptStaffByDeptID((string)Session["DeptID"]);

              SelectList list = new SelectList(DeptSatff, "UserID", "Username");

            ViewBag.DeptStaff = list;

            return View();
        }

        [HttpPost]
        public ActionResult SaveRepresentative(int UserID)
        {
            Users DepRepInfo = Data_Users.GetDepRepInfo((string)Session["DeptID"]);

            int prev_DepRep = DepRepInfo.UserID;

            int Confirm =  Data_Users.AssignRepresentative(UserID);

            if(Confirm != 0)
            {
                Data_Users.RemoveRepresentative(prev_DepRep);
            }
            return RedirectToAction("Assginrepresentative");
        }
    }
}