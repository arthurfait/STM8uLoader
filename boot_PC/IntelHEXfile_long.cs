﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

    public class IntelHEXfile_long {
    	
        public IntelHEXfile_long(string filename)
        {
        	if (filename.ToLower().EndsWith(".hex") != true) FileErrorMessages.Add(" Файл имеет расширение отличное от hex");
			
			try 
			{
				StreamReader sr = new StreamReader(filename);
				bool eof = false;
				int lineNumber = 0;
				while (!eof)
				{
					lineNumber++;
					HEXline line = new HEXline(sr.ReadLine(), lineNumber);
					//if (ErrorMessage == "") ErrorMessage = line.ErrorMessages;
					FileErrorMessages.AddRange(line.LineErrorMessages);
					if (line.CriticalErrors == true) CriticalError = true;
					
					switch (line.recordtype)
					{
						case HEXline.RecordType.ExtendedLinearAddress:
							ex_lin_address = (long)line.data[0];
							ex_lin_address = (long)ex_lin_address<<8;
							ex_lin_address = (long)(ex_lin_address + (long)line.data[1]);
							ex_lin_address = (long)ex_lin_address<<16;
							break;
							
						case HEXline.RecordType.DataRecord:
								//data.AddRange(line.data);
								AddressLineSorted.Add((long)(line.address+ex_lin_address), line.data);
								AddressLine.Add((long)(line.address+ex_lin_address), line.data);
								int ij=0;
								foreach( byte bt in line.data)
									{
											AddressByteSorted.Add((long)(line.address+ex_lin_address+ij), line.data[ij]);
											ij++;		
									}
								
							break;

						case HEXline.RecordType.EndOfFile:
							eof = true;
							break;
					}
					if (sr.EndOfStream)
					{
						eof = true;
					}
				}
				
				if(AddressByteSorted.Count != 0) {
					foreach( KeyValuePair<long, byte> dabs in AddressByteSorted )
					{
						Addresses.Add(dabs.Key);
						Bytes.Add(dabs.Value);
					}
					
					for (int i = 0; i < AddressByteSorted.Count; i++)
					{
						if (Addresses[i] < minAddress) minAddress = Addresses[i];
					}
					
					for (int i = 0; i < AddressByteSorted.Count; i++)
					{
						if (Addresses[i] > maxAddress) maxAddress = Addresses[i];
					}
				} else {FileErrorMessages.Add(" Файл не имеет адресов.");
				        minAddress = maxAddress = long.MinValue;}

				sr.Close();
				sr.Dispose();
			}
			catch(Exception ex)
			{
				ErrorMessage = ex.Message;
				CriticalError = true;
			}
        } //constructor IntelHEXfile_long(string filename)

       // public byte[] GetData()
       // {
        //    return data.ToArray();
        //}
        public long[] GetAddresses() { return Addresses.ToArray(); }
        public long GetMinAddress() { return minAddress; }
        public long GetMaxAddress() { return maxAddress; }
        public long GetBaseAddress() { return minAddress; }
        public byte[] GetBytes() { return Bytes.ToArray(); }
        public int GetCount() { return AddressByteSorted.Count; }
        public List<string> GetErrorMessages() {return FileErrorMessages;}
        //public string[] GetErrorMessages() { return FileErrorMessages.ToArray(); }
		public SortedDictionary<long, byte> GetAddressByteSorted() { return AddressByteSorted; }
		public SortedDictionary<long, byte[]> GetAddressLineSorted() { return AddressLineSorted; }
		public Dictionary<long, byte[]> GetAddressLine() { return AddressLine; }
		
        List<long> Addresses = new List<long>();
        List<byte> Bytes = new List<byte>();
        List<byte> data = new List<byte>();
		SortedDictionary<long, byte> AddressByteSorted = new SortedDictionary<long, byte>();
		SortedDictionary<long, byte[]> AddressLineSorted = new SortedDictionary<long, byte[]>();
		Dictionary<long, byte[]> AddressLine = new Dictionary<long, byte[]>();
		
        public class HEXline
        {
            public enum RecordType
            {
                DataRecord = 0,
                EndOfFile = 1,
                ExtendedSegmentAddress = 2,
                StartSegmentAddress = 3,
                ExtendedLinearAddress = 4,
                StartLinearAddress = 5
            }

            public HEXline(string s, int ln)
            {
				
                colon = s[0];
				//if (colon != ':') ErrorMessages = "Отсутствует двоеточие в начале одной из строк";
				if (colon != ':') LineErrorMessages.Add(" В начале строки " + ln + " отсутствует двоеточие");
                s = s.Substring(1);
                				
                try{
					length = long.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					s = s.Substring(2);
				}
				catch (Exception ex){
					//ErrorMessages = ex.Message;
					LineErrorMessages.Add(" В строке " + ln + " " + ex.Message);
					CriticalErrors = true;
				}
                try{
					address = long.Parse(s.Substring(0, 4), System.Globalization.NumberStyles.HexNumber);
					s = s.Substring(4);
				}
				catch (Exception ex){
					//ErrorMessages = ex.Message;
					LineErrorMessages.Add(" В строке " + ln + " " + ex.Message);
					CriticalErrors = true;
				}
                try{
					recordtypes = int.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					recordtype = (RecordType)recordtypes;
					s = s.Substring(2);
				}
				catch (Exception ex){
					LineErrorMessages.Add(" В строке " + ln + " " + ex.Message);
					CriticalErrors = true;
				}  
					data = new byte[length];
					for (int i = 0; i < length; i++)
					{
						
                		try{
							data[i] = byte.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
							s = s.Substring(2);
						}
						catch (Exception ex){
							LineErrorMessages.Add(" В строке " + ln + " " + ex.Message);
							CriticalErrors = true;
						}
					}
                try{
					checksum = byte.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					s = s.Substring(2);
				}
				catch (Exception ex){
					//ErrorMessages = ex.Message;
					LineErrorMessages.Add(" В строке " + ln + " " + ex.Message);
					CriticalErrors = true;
				}	
				for (int i = 0; i < length; i++){
						bytes += data[i];
				}
				if ((byte)(length + (address>>8) + (byte)address + recordtypes + bytes + checksum) != 0) 
						LineErrorMessages.Add(" В строке " + ln + " не совпадает контрольная сумма");
            }


            char colon;
            long length;
            public long address;

            public RecordType recordtype;
			public int recordtypes;
            public  byte[] data;
			public SortedDictionary<long, byte> AddressByteSorted;
			public SortedDictionary<long, byte[]> AddressLineSorted;
			public Dictionary<long, byte[]> AddressLine;
            byte checksum;
			byte bytes = 0;
			public List<string> LineErrorMessages = new List<string>();
			public bool CriticalErrors = false;

        }
        public long ex_lin_address;
	    //public string[] ErrorMessage;
		public List<string> FileErrorMessages = new List<string>();
		public string ErrorMessage ="";
		public bool CriticalError = false;
		public long minAddress = long.MaxValue;
		public long maxAddress = long.MinValue;
    } // class IntelHEXfile_long
// IntelHEXfile_long.cs
