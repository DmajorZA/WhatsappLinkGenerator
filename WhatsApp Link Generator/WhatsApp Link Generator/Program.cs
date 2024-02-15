using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace WhatsAppMessageLinkGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Contact> contacts = ReadContactsFromCSV("Path To Your CSV");

            if (contacts.Count > 0)
            {
                string messageTemplate =
@"Dear {Name},

We hope this message finds you well. As our wedding day draws near, we have encountered unexpected circumstances regarding our ceremony venue. Due to these changes, we have updated the venue details on our wedding website: LinkToWebsite/Ceremony (but don't worry, it is still on the same day!).

For those who have already RSVP'd, thank you for your prompt response! For those who haven't yet had the chance to RSVP, we kindly remind you to please take a moment to confirm your attendance or regrets at LinkToWebsite/Invite/{SecretKey}. Your RSVP helps us immensely with our planning and ensures that we can make your experience at our wedding truly special.

Your current RSVP status is ""{Confirmed}""{PlusOnes}.

Thank you for your cooperation and support as we prepare for this joyous occasion. We can't wait to celebrate with you!

Warm regards,
Shayne & Andrea";

                string filePath = "Path To TXT To Write Away To";

                GenerateWhatsAppLinksToFile(contacts, messageTemplate, filePath);

                Console.WriteLine($"WhatsApp message links generated and saved to {filePath}");
            }
            else
            {
                Console.WriteLine("No contacts found in the CSV file.");
            }

            Console.ReadLine();
        }

        static List<Contact> ReadContactsFromCSV(string filePath)
        {
            List<Contact> contacts = new List<Contact>();

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields.Length >= 5) // Assuming there are at least 5 fields in each row
                    {
                        contacts.Add(new Contact(fields[0], fields[1], fields[2], fields[3], fields[4]));
                    }
                    else
                    {
                        Console.WriteLine("Invalid row in CSV file.");
                    }
                }
            }

            return contacts;
        }

        static void GenerateWhatsAppLinksToFile(List<Contact> contacts, string messageTemplate, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (Contact contact in contacts)
                {
                    string plusOnesText = "";
                    if (contact.PlusOnes.ToUpper() != "NULL") //CSV file has NULL for empty plus ones
                    {
                        string[] plusOnes = contact.PlusOnes.Split(',');
                        if (plusOnes.Length > 0)
                        {
                            string lastPlusOne = plusOnes[plusOnes.Length - 1];
                            if (!string.IsNullOrEmpty(lastPlusOne))
                            {
                                plusOnesText = ", with ";
                                if (plusOnes.Length > 1)
                                {
                                    plusOnesText += string.Join(", ", plusOnes, 0, plusOnes.Length - 1);
                                    plusOnesText += ", and " + lastPlusOne;
                                }
                                else
                                {
                                    plusOnesText += lastPlusOne;
                                }
                            }
                        }
                    }

                    string message = messageTemplate.Replace("{Name}", contact.Name)
                                                    .Replace("{SecretKey}", contact.SecretKey)
                                                    .Replace("{Confirmed}", contact.Confirmed)
                                                    .Replace("{PlusOnes}", plusOnesText);

                    string whatsappLink = $"https://wa.me/{contact.ContactNumber}?text={Uri.EscapeDataString(message)}";
                    writer.WriteLine($"WhatsApp link for {contact.Name}: {whatsappLink}");
                }
            }
        }
    }

    class Contact
    {
        public string Name { get; }
        public string PlusOnes { get; }
        public string ContactNumber { get; }
        public string SecretKey { get; }
        public string Confirmed { get; }

        public Contact(string name, string plusOnes, string contactNumber, string secretKey, string confirmed)
        {
            Name = name;
            PlusOnes = plusOnes;
            ContactNumber = contactNumber;
            SecretKey = secretKey;
            Confirmed = confirmed;
        }
    }
}
