using System;
using System.Linq;
using System.Text;
using OnBoarding.Models;
using System.Security.Cryptography;

namespace OnBoarding.Services
{
    public static class Functions
    {
        //1.
        //Function Generate MD5
        public static string GenerateMD5Hash(string yourString)
        {
            return string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(yourString)).Select(s => s.ToString("x2")));
        }

        //2.
        //Log LogAuditTrail
        public static bool LogAuditTrail(int _EntityId, string _ActionType, string _EntityTable, string _EntityUId, string _DoneBy, string _EntityName, string _EntityEmail, string _EntityPhone)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var newDeletedEntry = db.AuditTrails.Create();
                    newDeletedEntry.EntityId = _EntityId;
                    newDeletedEntry.ActionType = _ActionType;
                    newDeletedEntry.EntityTable = _EntityTable;
                    newDeletedEntry.EntityUId = _EntityUId;
                    newDeletedEntry.DoneBy = _DoneBy;
                    newDeletedEntry.DateCreated = DateTime.Now;
                    newDeletedEntry.EntityName = _EntityName;
                    newDeletedEntry.EntityEmail = _EntityEmail;
                    newDeletedEntry.EntityPhone = _EntityPhone;
                    db.AuditTrails.Add(newDeletedEntry);
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch(Exception)
                {
                    return false;
                }
            }
        }
    }
}