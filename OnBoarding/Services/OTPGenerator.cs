using System;

namespace OnBoarding.Services
{
    public class OTPGenerator
    {
        public static string GetUniqueKey(int maxSize)
        {

            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string numbers = "1234567890";
            string characters = numbers;

            characters += alphabets + numbers;
           
            int length = maxSize;
            string otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                }
                while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
    }

    public class OTPGeneratorReG
    {
        public static string GetUniqueKey(int maxSize)
        {

            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int length = maxSize;
            string otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                }
                while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
    }

    public class Shuffle
    {
        static System.Random rnd = new System.Random();

        static void Fisher_Yates(int[] array)
        {
            int arraysize = array.Length;
            int random;
            int temp;

            for (int i = 0; i < arraysize; i++)
            {
                random = i + (int)(rnd.NextDouble() * (arraysize - i));

                temp = array[random];
                array[random] = array[i];
                array[i] = temp;
            }
        }

        public static string StringMixer(string s)
        {
            string output = "";
            int arraysize = s.Length;
            int[] randomArray = new int[arraysize];

            for (int i = 0; i < arraysize; i++)
            {
                randomArray[i] = i;
            }

            Fisher_Yates(randomArray);
            for (int i = 0; i < arraysize; i++)
            {
                output += s[randomArray[i]];
            }

            return output;
        }
    }
}
