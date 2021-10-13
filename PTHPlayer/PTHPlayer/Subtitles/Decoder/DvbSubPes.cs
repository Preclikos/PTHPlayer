using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PTHPlayer.Subtitles.Decoder
{
    public class DvbSubPes
    {
        public static readonly int DefaultScreenWidth = 720;
        public static readonly int DefaultScreenHeight = 576;

        public int Length { get; }
        public int? SubPictureStreamId { get; }

        private readonly byte[] _dataBuffer;

        public DvbSubPes(int index, byte[] buffer)
        {
            int start = index;
            Length = index + 1;

            SubtitleSegments = new List<SubtitleSegment>();
            ClutDefinitions = new List<ClutDefinitionSegment>();
            RegionCompositions = new List<RegionCompositionSegment>();
            PageCompositions = new List<PageCompositionSegment>();
            ObjectDataList = new List<ObjectDataSegment>();
            EndOfDisplaySetSegmentList = new List<EndOfDisplaySetSegment>();

            // Find length of segments
            index = start;
            var ss = new SubtitleSegment(buffer, index);
            while (ss.SyncByte == 0b00001111)
            {
                SubtitleSegments.Add(ss);
                index += 6 + ss.SegmentLength;
                if (index + 6 < buffer.Length)
                {
                    ss = new SubtitleSegment(buffer, index);
                }
                else
                {
                    ss.SyncByte = 0b11111111;
                }
            }
            Length = index;
            int size = index - start;
            _dataBuffer = new byte[size];
            Buffer.BlockCopy(buffer, start, _dataBuffer, 0, _dataBuffer.Length);

            // Parse segments
            index = 0;
            ss = new SubtitleSegment(_dataBuffer, index);
            while (ss.SyncByte == 0b00001111)
            {
                SubtitleSegments.Add(ss);
                if (ss.ClutDefinition != null)
                {
                    ClutDefinitions.Add(ss.ClutDefinition);
                }
                else if (ss.RegionComposition != null)
                {
                    RegionCompositions.Add(ss.RegionComposition);
                }
                else if (ss.PageComposition != null)
                {
                    PageCompositions.Add(ss.PageComposition);
                }
                else if (ss.ObjectData != null)
                {
                    ObjectDataList.Add(ss.ObjectData);
                }
                else if (ss.EndOfDisplaySet != null)
                {
                    EndOfDisplaySetSegmentList.Add(ss.EndOfDisplaySet);
                }

                index += 6 + ss.SegmentLength;
                if (index + 6 < _dataBuffer.Length)
                {
                    ss = new SubtitleSegment(_dataBuffer, index);
                }
                else
                {
                    ss.SyncByte = 0b11111111;
                }
            }
        }

        public bool IsDvbSubPicture => SubPictureStreamId.HasValue && SubPictureStreamId.Value == 32;

        public bool IsTeletext => DataIdentifier == 16;

        public int DataIdentifier
        {
            get
            {
                if (_dataBuffer == null || _dataBuffer.Length < 2)
                {
                    return 0;
                }

                return _dataBuffer[0];
            }
        }

        public int SubtitleStreamId
        {
            get
            {
                if (_dataBuffer == null || _dataBuffer.Length < 2)
                {
                    return 0;
                }

                return _dataBuffer[1];
            }
        }

        public List<SubtitleSegment> SubtitleSegments { get; set; }
        public List<ClutDefinitionSegment> ClutDefinitions { get; set; }
        public List<RegionCompositionSegment> RegionCompositions { get; set; }
        public List<PageCompositionSegment> PageCompositions { get; set; }
        public List<ObjectDataSegment> ObjectDataList { get; set; }
        public List<EndOfDisplaySetSegment> EndOfDisplaySetSegmentList { get; set; }

        public void ParseSegments()
        {
            if (SubtitleSegments != null)
            {
                return;
            }

            SubtitleSegments = new List<SubtitleSegment>();
            ClutDefinitions = new List<ClutDefinitionSegment>();
            RegionCompositions = new List<RegionCompositionSegment>();
            PageCompositions = new List<PageCompositionSegment>();
            ObjectDataList = new List<ObjectDataSegment>();

            int index = 2;
            var ss = new SubtitleSegment(_dataBuffer, index);
            while (ss.SyncByte == 0b00001111)
            {
                SubtitleSegments.Add(ss);
                if (ss.ClutDefinition != null)
                {
                    ClutDefinitions.Add(ss.ClutDefinition);
                }
                else if (ss.RegionComposition != null)
                {
                    RegionCompositions.Add(ss.RegionComposition);
                }
                else if (ss.PageComposition != null)
                {
                    PageCompositions.Add(ss.PageComposition);
                }
                else if (ss.ObjectData != null)
                {
                    ObjectDataList.Add(ss.ObjectData);
                }

                index += 6 + ss.SegmentLength;
                if (index + 6 < _dataBuffer.Length)
                {
                    ss = new SubtitleSegment(_dataBuffer, index);
                }
                else
                {
                    ss.SyncByte = 0b11111111;
                }
            }
        }

        private ClutDefinitionSegment GetClutDefinitionSegment(ObjectDataSegment ods)
        {
            foreach (var rcs in RegionCompositions)
            {
                foreach (var o in rcs.Objects)
                {
                    if (o.ObjectId == ods.ObjectId)
                    {
                        foreach (var cds in ClutDefinitions)
                        {
                            if (cds.ClutId == rcs.RegionClutId)
                            {
                                return cds;
                            }
                        }
                    }
                }
            }

            if (ClutDefinitions.Count > 0)
            {
                return ClutDefinitions[0];
            }

            return null; // TODO: Return default clut
        }

        public SKPoint GetImagePosition(ObjectDataSegment ods)
        {
            if (SubtitleSegments == null)
            {
                ParseSegments();
            }

            var p = new SKPoint(0, 0);

            foreach (var rcs in RegionCompositions)
            {
                foreach (var o in rcs.Objects)
                {
                    if (o.ObjectId == ods.ObjectId)
                    {
                        foreach (var cds in PageCompositions)
                        {
                            foreach (var r in cds.Regions)
                            {
                                if (r.RegionId == rcs.RegionId)
                                {
                                    p.X = r.RegionHorizontalAddress + o.ObjectHorizontalPosition;
                                    p.Y = r.RegionVerticalAddress + o.ObjectVerticalPosition;
                                    return p;
                                }
                            }
                        }
                        p.X = o.ObjectHorizontalPosition;
                        p.Y = o.ObjectVerticalPosition;
                    }
                }
            }

            return p;
        }
        /*
        public Bitmap AddBitmapBorder(Bitmap InputBitmap, Color BorderColour)
        {

            if ((InputBitmap.Width < 5) || (InputBitmap.Height < 5))
            {
                return InputBitmap;
            }

            Bitmap bmp = new Bitmap(InputBitmap, new System.Drawing.Size(InputBitmap.Width, InputBitmap.Height));

            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp);

            gr.DrawLine(new Pen(BorderColour, 1), new Point(0, 0), new Point(0, InputBitmap.Height - 1));
            gr.DrawLine(new Pen(BorderColour, 1), new Point(0, 0), new Point(InputBitmap.Width - 1, 0));
            gr.DrawLine(new Pen(BorderColour, 1), new Point(0, InputBitmap.Height - 1), new Point(InputBitmap.Width - 1, InputBitmap.Height - 1));
            gr.DrawLine(new Pen(BorderColour, 1), new Point(InputBitmap.Width - 1, 0), new Point(InputBitmap.Width - 1, InputBitmap.Height - 1));

            return bmp;
        }*/

        public SKBitmap GetImage(ObjectDataSegment ods, bool AddBorder = false)
        {
            if (SubtitleSegments == null)
            {
                ParseSegments();
            }

            // this caches the image, removing so border switching works, do I need to clean up memory?
            if (ods.Image != null)
            {
                //return ods.Image;
                ods.Image.Dispose();
            }

            var cds = GetClutDefinitionSegment(ods);
            ods.DecodeImage(_dataBuffer, ods.BufferIndex, cds);
            /*
            if (AddBorder)
            {
                return AddBitmapBorder(ods.Image, Color.White);
            }
            else
            {*/
            return ods.Image;
            //}
        }

        public SKBitmap GetImageFull(bool ShowObjectBorders = false)
        {
            if (SubtitleSegments == null)
            {
                ParseSegments();
            }

            int width = DefaultScreenWidth;
            int height = DefaultScreenHeight;

            var segments = SubtitleSegments;
            foreach (var ss in segments)
            {
                if (ss.DisplayDefinition != null)
                {
                    width = ss.DisplayDefinition.DisplayWidth;
                    height = ss.DisplayDefinition.DisplayHeight;
                }
            }

            var bmp = new SKBitmap(width, height);
            var canavas = new SKCanvas(bmp);
            foreach (var ods in ObjectDataList)
            {
                var odsImage = GetImage(ods, ShowObjectBorders);
                if (odsImage != null)
                {
                    var odsPoint = GetImagePosition(ods);

                    canavas.DrawBitmap(odsImage, odsPoint);

                }
            }
            return bmp;
        }

        public int PageTimeOut()
        {
            var page = PageCompositions.FirstOrDefault();
            if(page != null)
            {
                return page.PageTimeOut;
            }
            return 2;
        }

        public bool ContainsData()
        {
            return ObjectDataList.Any();
        }
    }
}
