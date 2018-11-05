﻿namespace Soulseek.NET.Messaging.Responses
{
    using Soulseek.NET.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class SharesResponse
    {
        public int DirectoryCount { get; private set; }
        public IEnumerable<Directory> Directories => DirectoryList.AsReadOnly();

        private List<Directory> DirectoryList { get; set; } = new List<Directory>();

        internal SharesResponse()
        {
        }

        public static SharesResponse Parse(Message message)
        {
            var reader = new MessageReader(message);

            if (reader.Code != MessageCode.PeerSharesResponse)
            {
                throw new MessageException($"Message Code mismatch creating Peer Shares Response (expected: {(int)MessageCode.PeerSharesResponse}, received: {(int)reader.Code}");
            }

            try
            {
                reader.Decompress();
            }
            catch (Exception)
            {
                // discard result if it fails to decompress
                return null;
            }

            var response = new SharesResponse
            {
                DirectoryCount = reader.ReadInteger(),
            };

            for (int i = 0; i < response.DirectoryCount; i++)
            {
                var dir = new Directory
                {
                    Directoryname = reader.ReadString(),
                    FileCount = reader.ReadInteger(),
                };

                for (int j = 0; j < dir.FileCount; j++)
                {
                    var file = new File
                    {
                        Code = reader.ReadByte(),
                        Filename = reader.ReadString(),
                        Size = reader.ReadLong(),
                        Extension = reader.ReadString(),
                        AttributeCount = reader.ReadInteger()
                    };

                    for (int k = 0; k < file.AttributeCount; k++)
                    {
                        var attribute = new FileAttribute
                        {
                            Type = (FileAttributeType)reader.ReadInteger(),
                            Value = reader.ReadInteger()
                        };
                        ((List<FileAttribute>)file.Attributes).Add(attribute);
                    }

                    dir.FileList.Add(file);
                }

                response.DirectoryList.Add(dir);
            }


            return response;
        }

        public class Directory
        {
            public string Directoryname { get; set; }
            public int FileCount { get; set; }
            public IEnumerable<File> Files => FileList.AsReadOnly();

            internal List<File> FileList { get; set; } = new List<File>();

        }
    }
}