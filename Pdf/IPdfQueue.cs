﻿using System;

namespace Pdf.Storage.Pdf
{
    public interface IPdfQueue
    {
        void CreatePdf(Guid pdfEntityId);
    }
}