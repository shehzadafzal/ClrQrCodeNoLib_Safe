using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Server;
using SkiaSharp.QrCode;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString SqlFunction1()
    {
        // Put your code here
        return new SqlString (string.Empty);
    }

    [SqlFunction(DataAccess = DataAccessKind.None, IsDeterministic = true)]
    public static SqlBytes GetQrCode(SqlString text,int Scale)
    {


        string content = text.ToString();
        int[,] matrix = null;
        using (var generator = new QRCodeGenerator())
        {
            // Generate QrCode
            QRCodeData data = generator.CreateQrCode(content, ECCLevel.L, quietZoneSize: 1);

            UserDefinedFunctions udf = new UserDefinedFunctions();
            matrix = udf.QrCodeDateToIntDArray(data, Scale);
        }

        //int[,] matrix = GenerateQRCodeMatrix(data);

        /*
        matrix = new int[3, 5] { 
            { 1,1, 1 ,1,1},
            { 0, 1, 0,1,0},
            { 1, 0, 0,0,1 }
               };

        */


        int width = matrix.GetLength(1);// 2;
        int height = matrix.GetLength(0);// 2;
        int padedWidth = (int)(4 * Math.Ceiling((width * 3) / 4.0));
        int deltaPad = padedWidth - width * 3;
        int imageSize = (width * height * 3 + (deltaPad) * height); // 3 bytes per pixel (RGB)

        // Create the file header
        byte[] header = new byte[54];
        header[0] = 0x42; // B
        header[1] = 0x4D; // M
        BitConverter.GetBytes(imageSize + 54).CopyTo(header, 2); // file size
        header[10] = 54; // data offset
        header[14] = 40; // info header size
        BitConverter.GetBytes(width).CopyTo(header, 18);
        BitConverter.GetBytes(height).CopyTo(header, 22);
        header[26] = 1; // color planes
        header[28] = 24; // bits per pixel

        byte[] imageData = new byte[imageSize];
        //int i = 0;


        //for (int j = height - 1; j >=0; j--) // rows
        for (int j = 0; j < height; j++) // rows
        {
            int i = 0;
            int pixLoc = 0;
            for (i = 0; i < width; i = i + 1) // cols
            {
                //int bit = matrix[i, height-j-1];
                int bit = matrix[height - j - 1, i];
                pixLoc = i * 3 + j * width * 3 + j * deltaPad;
                if (bit == 0)
                {
                    imageData[pixLoc] = 0x01; // blue
                    imageData[pixLoc + 1] = 0x02; // green
                    imageData[pixLoc + 2] = 0x03; // red                    
                }
                else
                {
                    imageData[pixLoc] = 0xfd; // blue
                    imageData[pixLoc + 1] = 0xfe; // green
                    imageData[pixLoc + 2] = 0xff; // red                    
                }
            }

            // PADDING ZEROS
            for (int l = 0; l < deltaPad; l++) imageData[pixLoc + 3 + l] = 0x02; // red

        }

        // Write the file header and image data to a file
        /*
        using (FileStream fs = new FileStream(@"c:\del\image_12.bmp", FileMode.Create))
        {
            fs.Write(header, 0, header.Length);
            fs.Write(imageData, 0, imageData.Length);
        }
        */

        int headerLength = header.Length;
        int imageDataLength = imageData.Length;
        byte[] result = new byte[headerLength + imageDataLength];

        Buffer.BlockCopy(header, 0, result, 0, headerLength);
        Buffer.BlockCopy(imageData, 0, result, headerLength, imageDataLength);



        // Return the byte array as a SQL byte array
        SqlBytes sqlBytes = new SqlBytes(result);
        return sqlBytes;
    }


    public int[,] QrCodeDateToIntDArray(QRCodeData data, int scaleFactor)
    {
        if (scaleFactor == 0) scaleFactor = 1;

        int[,] matrix;
        int Size = data.ModuleMatrix.Count() * scaleFactor;

        matrix = new int[Size, Size];
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                //   MatrixInt[j, i] = (int)(data.ModuleMatrix[i][j] == 0 ? 0 : 1);
                int i_shrink = (int)(i / (scaleFactor * 1.0));
                int j_shrink = (int)(j / (scaleFactor * 1.0));
                //bool b = data.ModuleMatrix[i][j];
                bool b = data.ModuleMatrix[i_shrink][j_shrink];
                int v = (b ? 1 : 0);
                matrix[j, i] = v;


            }
        }

        return matrix;
    }


}




