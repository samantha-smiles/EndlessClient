﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Pub;

namespace EOLib.IO.Repositories
{
    public interface IPubFileRepository : IEIFFileRepository, IENFFileRepository, IESFFileRepository, IECFFileRepository
    {
    }

    public interface IEIFFileRepository
    {
        IPubFile<EIFRecord> EIFFile { get; set; }
    }

    public interface IENFFileRepository
    {
        IPubFile<ENFRecord> ENFFile { get; set; }
    }

    public interface IESFFileRepository
    {
        IPubFile<ESFRecord> ESFFile { get; set; }
    }

    public interface IECFFileRepository
    {
        IPubFile<ECFRecord> ECFFile { get; set; }
    }
}
