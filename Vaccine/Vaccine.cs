using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Vaccine
{
    /// <summary>
    /// This class is responsible to decrypt all the given directories content.
    /// </summary>
    class Vaccine
    {
        private static List<string> directoriesToDecrypt = new List<string>();
        private static string password = "encryptpassword";

        /// <summary>
        /// Program entry point
        /// </summary>
        static void Main(string[] args)
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // BE CAREFULL WHEN REMOVING THESE COMMENTS
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            //directoriesToDecrypt.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            //directoriesToDecrypt.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            //directoriesToDecrypt.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            //directoriesToDecrypt.Add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            Decrypt();
        }

        /// <summary>
        /// Function responsible to go through all the directories that are supposed to be decrypted
        /// </summary>
        private static void Decrypt()
        {
            foreach (string path in directoriesToDecrypt)
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

        /// <summary>
        /// Function that will decrypt a file and get it back to the original state
        /// </summary>
        private static void ProcessFile(string path)
        {
            if (!path.Contains(".ransomflow"))
            {
                return;
            }

            string[] pathTemp = path.Split('\\');
            string fileName = pathTemp[pathTemp.Length - 1];

            string[] fileNameTemp = fileName.Split('.');
            fileNameTemp[fileNameTemp.Length - 1] = "";

            string tempDecryptedFileName = String.Join(".", fileNameTemp);
            pathTemp[pathTemp.Length - 1] = tempDecryptedFileName;

            string decryptedPath = String.Join("\\", pathTemp);

            Console.WriteLine("Decrypting file '{0}' to '{1}'", path, decryptedPath);

            FileDecrypt(path, decryptedPath, password);
            File.Delete(path);
        }

        /// <summary>
        /// Decrypts a file using the AES when given the correct password
        /// More info: https://ourcodeworld.com/articles/read/471/how-to-encrypt-and-decrypt-files-using-the-aes-encryption-algorithm-in-c-sharp
        /// </summary>
        private static void FileDecrypt(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch { }

            try
            {
                cs.Close();
            }
            catch { }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }
        }
    }
}
