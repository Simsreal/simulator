using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class XmlReaderUtility
{
    public static Dictionary<string, string> ReadXmlAttributes(string filePath)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        XmlDocument xmlDoc = new XmlDocument();

        try
        {
            xmlDoc.Load(filePath);
            XmlElement root = xmlDoc.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Attributes != null)
                {
                    foreach (XmlAttribute attr in node.Attributes)
                    {
                        data[attr.Name] = attr.Value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred while reading XML: {ex.Message}");
        }

        return data;
    }
}