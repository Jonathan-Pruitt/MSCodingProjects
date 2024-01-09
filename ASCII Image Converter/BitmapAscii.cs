using System;
using System.Collections.Generic;
using System.Drawing;


class BitmapAscii {
    #region CONSTRUCTOR
    #endregion

    #region FIELDS
    private string _asciiImage = "";
    private int _kernelSpan = 3;
    #endregion

    #region PROPERTIES
    #endregion

    #region PUBLIC METHODS    
    public string Asciitize(BitmapMaker incomingBmp) {

        double bmpWidth = incomingBmp.Width;
        double bmpHeight = incomingBmp.Height;
        double bmpWtoHRatio = bmpWidth / bmpHeight;
        //int krnWidth = (int)Math.Ceiling(((double)bmpWidth / (45 * bmpWtoHRatio)) * .55);
        int krnWidth = (int)Math.Ceiling(((double)bmpWidth / (250 * bmpWtoHRatio)) * .55);
        int krnHeight = (int)Math.Ceiling((double)bmpHeight / (250 * bmpWtoHRatio));
        //int krnHeight = (int)Math.Ceiling((double)bmpHeight / (45 * bmpWtoHRatio));
        double normalized;

        for (int y = 0; y < bmpHeight; y += krnHeight) {
            for (int x = 0; x < bmpWidth; x += krnWidth) {
                
                byte[] data = incomingBmp.GetPixelData(x, y);
                normalized = AveragePixel(data[0], data[1], data[2]);
                if (x + 2 < bmpWidth && y + 2 < bmpHeight) {
                    data = incomingBmp.GetPixelData(x + 1, y + 1);
                    normalized += AveragePixel(data[0], data[1], data[2]);
                    data = incomingBmp.GetPixelData(x + 2, y + 2);
                    normalized += AveragePixel(data[0], data[1], data[2]);
                    data = incomingBmp.GetPixelData(x, y + 1);
                    normalized += AveragePixel(data[0], data[1], data[2]);
                    data = incomingBmp.GetPixelData(x + 1, y);
                    normalized += AveragePixel(data[0], data[1], data[2]);
                    data = incomingBmp.GetPixelData(x, y + 2);
                    normalized += AveragePixel(data[0], data[1], data[2]);
                    data = incomingBmp.GetPixelData(x + 2, y);
                    normalized += AveragePixel(data[0], data[1], data[2]);
                    normalized = normalized / 7;

                } 

                _asciiImage += GrayToString(normalized);

            }
            _asciiImage += "\n";
        }
        return _asciiImage;

    }//end method   

    public override string ToString() {
        return _asciiImage;
    }//end method
    #endregion
    
    #region PRIVATE METHODS
    private double AveragePixel(byte r, byte g, byte b) {
        double averageColorValue = ((r + g + b) / 3);
        return (averageColorValue / 255);
    }//end method

    private double AveragePixel(Color pixelColor) {
        double averageColorValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;  
        return (averageColorValue / 255);
    }//end method

    private double AverageColor(List<Color> colors) {
        double totalColorValue = 0.0;
        double averageColorValue = 0.0;
        foreach (Color color in colors) {
            totalColorValue += color.R + color.G + color.B;
        }//end for
        averageColorValue = totalColorValue / colors.Count;
        return averageColorValue / 255;
    }//end method

    private string GrayToString(double normalizedValue) {
        if        (normalizedValue < .10) {
            return "@";
        } else if (normalizedValue < .20) {
            return "%";
        } else if (normalizedValue < .30) {
            return "#";
        } else if (normalizedValue < .40) {
            return "*";
        } else if (normalizedValue < .50) {
            return "+";
        } else if (normalizedValue < .60) {
            return "=";
        } else if (normalizedValue < .70) {
            return "-";
        } else if (normalizedValue < .80) {
            return ":";
        } else if (normalizedValue < .90) {
            return ".";
        } else {
            return " ";
        }
    }//end method
    #endregion
}
