using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace OnBoarding.Services
{
    public static class Functions
    {
        //Function Generate MD5
        public static string GenerateMD5Hash(string yourString)
        {
            return string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(yourString)).Select(s => s.ToString("x2")));
        }
    }
}