using System.Collections.Generic;

namespace PTHPlayer.Subtitles.Decoder
{
    public class PageCompositionSegment
    {
        public int PageTimeOut { get; set; }
        public int PageVersionNumber { get; set; }
        public int PageState { get; set; }
        public List<PageCompositionSegmentRegion> Regions;


        public string PageStateString
        {
            get
            {
                switch (PageState)
                {
                    case 0:
                        return "Normal Case (page update)";
                    case 1:
                        return "Aquisitiion Point (page refresh)";
                    case 2:
                        return "Mode Change (new page";
                    case 3:
                        return "reserved";
                    default:
                        return "(unknown page state)";
                }
            }
        }
        public PageCompositionSegment(byte[] buffer, int index, int regionLength)
        {
            PageTimeOut = buffer[index];
            PageVersionNumber = buffer[index + 1] >> 4;
            PageState = (buffer[index + 1] & 0b00001100) >> 2;
            Regions = new List<PageCompositionSegmentRegion>();
            int i = 0;
            while (i < regionLength && i + index < buffer.Length)
            {
                var pageCompositionSegmentRegion = new PageCompositionSegmentRegion { RegionId = buffer[i + index + 2] };
                i += 2;
                pageCompositionSegmentRegion.RegionHorizontalAddress = Helper.GetEndianWord(buffer, i + index + 2);
                i += 2;
                pageCompositionSegmentRegion.RegionVerticalAddress = Helper.GetEndianWord(buffer, i + index + 2);
                i += 2;
                Regions.Add(pageCompositionSegmentRegion);
            }
        }
    }

}