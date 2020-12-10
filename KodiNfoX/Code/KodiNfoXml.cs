using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
namespace KodiNfoX.Code
{
    public class Actor
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }

    public class KodiNfoXml
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string[] Genre {get; set;}
        public Actor[] Actor { get; set; }
        public string Year {get; set;}
        public string Thumb { get; set; }
        public string Plot { get; set; }
        public string[] Director { get; set; }
        public string[] Writer { get; set; }
        public string Rating { get; set; }
        public string[] Producer { get; set; }
        public KodiNfoXml(string title, string sorttitle, string[] genre, string year, string thumb, string plot, Actor[] actor, string[] director, string[] writer, string rating, string[] producer)
        {
            this.Title = title;
            this.SortTitle = sorttitle;
            this.Genre = genre;
            this.Year = year;
            this.Thumb = thumb;
            this.Plot = plot;
            this.Actor = actor;
            this.Director = director;
            this.Writer = writer;
            this.Rating = rating;
            this.Producer = producer;
        }

        public XDocument ToXDocument()
        {            
            XDocument xd = new XDocument(new XDeclaration("1.0", "utf-8", null));

            XElement xeRoot = new XElement("movie");
            xd.Add(xeRoot);

            XElement xeTitle = new XElement("title");
            xeTitle.Value = this.Title ?? string.Empty;
            xeRoot.Add(xeTitle);

            XElement xeSortTitle = new XElement("sorttitle");
            xeSortTitle.Value = this.SortTitle ?? string.Empty;
            xeRoot.Add(xeSortTitle);

            if (this.Genre != null)
            {
                foreach (var genre in this.Genre)
                {
                    if ( !string.IsNullOrEmpty(genre) && !string.IsNullOrWhiteSpace(genre)) {
                        XElement xeGenre = new XElement("genre");
                        xeGenre.Value = genre;
                        xeRoot.Add(xeGenre);
                    }
                }
            }

            XElement xeYear = new XElement("year");
            xeYear.Value = this.Year ?? string.Empty;
            xeRoot.Add(xeYear);

            XElement xeThumb = new XElement("thumb");
            xeThumb.Value = this.Thumb ?? string.Empty;
            xeRoot.Add(xeThumb);

            XElement xePlot = new XElement("plot");
            xePlot.Value = this.Plot ?? string.Empty;
            xeRoot.Add(xePlot);

            XElement xeOutline = new XElement("outline");
            xeOutline.Value = this.Plot ?? string.Empty;
            xeRoot.Add(xeOutline);

            XElement xeRating = new XElement("rating");
            xeRating.Value = this.Rating ?? string.Empty;
            xeRoot.Add(xeRating);

            if (this.Director != null)
            {
                foreach (var director in this.Director)
                {
                    XElement xeDirector = new XElement("director");
                    xeDirector.Value = director ?? string.Empty;
                    xeRoot.Add(xeDirector);
                }
            }

            if (this.Writer != null)
            {
                foreach (var writer in this.Writer)
                {
                    XElement xeWriter = new XElement("writer");
                    xeWriter.Value = writer ?? string.Empty;
                    xeRoot.Add(xeWriter);
                }
            }

            if (this.Producer != null)
            {
                foreach (var producer in this.Producer)
                {
                    XElement xeProducer = new XElement("producer");
                    xeProducer.Value = producer ?? string.Empty;
                    xeRoot.Add(xeProducer);
                }
            }

            if (this.Actor != null)
            {
                foreach (var actor in this.Actor)
                {
                    if (actor != null)
                    {
                        if ( !string.IsNullOrEmpty(actor.Name) && !string.IsNullOrWhiteSpace(actor.Name) &&
                             !string.IsNullOrEmpty(actor.Role) && !string.IsNullOrWhiteSpace(actor.Role) ) 
                        {
                            XElement xeActor = new XElement("actor");
                        
                            XElement xeName = new XElement("name");
                            xeName.Value = actor.Name;
                            xeActor.Add(xeName);                                                

                            XElement xeRole = new XElement("role");
                            xeRole.Value = actor.Role;
                            xeActor.Add(xeRole);
                    
                            xeRoot.Add(xeActor);                        
                        }
                    }
                }
            }

            return xd;
        }

        internal static bool CheckForMissingInformation(XDocument xd, DeleteNfoParams deleteNfoParams)
        {
            bool bResult = false;

            if (!bResult && deleteNfoParams.Actor)
            {
                if (xd.Root.Elements("actor") != null && xd.Root.Elements("actor").Count() > 0)
                {
                    var items = xd.Root.Elements("actor");
                    if (items.Count() == 0)
                    {
                        bResult = true;
                    }
                    foreach (var actor in items )
                    {
                        var name = actor.Element("name");
                        var role = actor.Element("role");
                        if (name != null || role != null)
                        {
                            if (name != null && (string.IsNullOrEmpty(name.Value) || string.IsNullOrEmpty(role.Value)))
                            {
                                bResult = true;
                                break;
                            }
                            else if (role != null && (string.IsNullOrEmpty(role.Value) || string.IsNullOrWhiteSpace(role.Value)))
                            {
                                bResult = true;
                                break;
                            }
                        }
                        else if ( name == null || role == null)
                        {
                            bResult = true;
                            break;
                        }
                    }                    
                }
                else
                {
                    bResult = true;
                }
            }

            if (!bResult && deleteNfoParams.Director)
            {
                if (xd.Root.Elements("director") != null && xd.Root.Elements("director").Count() > 0)
                {
                    foreach (var director in xd.Root.Elements("director"))
                    {
                        if (string.IsNullOrEmpty(director.Value) || string.IsNullOrWhiteSpace(director.Value))
                        {
                            bResult = true;
                            break;
                        }
                    }
                }
                else
                {
                    bResult = true;
                }
            }

            if (!bResult && deleteNfoParams.Writer)
            {
                if (xd.Root.Elements("writer") != null && xd.Root.Elements("writer").Count() > 0)
                {
                    foreach (var director in xd.Root.Elements("writer"))
                    {
                        if (string.IsNullOrEmpty(director.Value) || string.IsNullOrWhiteSpace(director.Value))
                        {
                            bResult = true;
                            break;
                        }
                    }
                }
                else
                {
                    bResult = true;
                }
            }

            if (!bResult && deleteNfoParams.Producer)
            {
                if (xd.Root.Elements("producer") != null && xd.Root.Elements("producer").Count() > 0)
                {
                    foreach (var director in xd.Root.Elements("producer"))
                    {
                        if (string.IsNullOrEmpty(director.Value) || string.IsNullOrWhiteSpace(director.Value))
                        {
                            bResult = true;
                            break;
                        }
                    }
                }
                else
                {
                    bResult = true;
                }
            }

            if (!bResult && deleteNfoParams.Genre)
            {
                if (xd.Root.Elements("genre") != null && xd.Root.Elements("genre").Count() > 0)
                {                
                    foreach (var genre in xd.Root.Elements("genre"))
                    {
                        if (string.IsNullOrEmpty(genre.Value) || string.IsNullOrWhiteSpace(genre.Value))
                        {
                            bResult = true;
                            break;
                        }
                    }                    
                }
                else
                {
                    bResult = true;
                }
            }

            if (!bResult && (xd.Root.Element("plot") == null || string.IsNullOrWhiteSpace(xd.Root.Element("plot").Value) || string.IsNullOrEmpty(xd.Root.Element("plot").Value)) && deleteNfoParams.PlotOutline)
            {
                bResult = true;
            }

            if (!bResult && (xd.Root.Element("outline") == null || string.IsNullOrWhiteSpace(xd.Root.Element("outline").Value) || string.IsNullOrEmpty(xd.Root.Element("outline").Value)) && deleteNfoParams.PlotOutline)
            {
                bResult = true;
            }

            if (!bResult && (xd.Root.Element("thumb") == null || string.IsNullOrWhiteSpace(xd.Root.Element("thumb").Value) || string.IsNullOrEmpty(xd.Root.Element("thumb").Value)) && deleteNfoParams.ThumbPoster)
            {
                bResult = true;
            }

            if (!bResult && (xd.Root.Element("title") == null || string.IsNullOrWhiteSpace(xd.Root.Element("title").Value) || string.IsNullOrEmpty(xd.Root.Element("title").Value)) && deleteNfoParams.TitleSortTitle)
            {
                bResult = true;
            }

            if (!bResult && (xd.Root.Element("sorttitle") == null || string.IsNullOrWhiteSpace(xd.Root.Element("sorttitle").Value) || string.IsNullOrEmpty(xd.Root.Element("sorttitle").Value)) && deleteNfoParams.TitleSortTitle)
            {
                bResult = true;
            }

            if (!bResult && (xd.Root.Element("rating") == null || string.IsNullOrWhiteSpace(xd.Root.Element("rating").Value) || string.IsNullOrEmpty(xd.Root.Element("rating").Value)) && deleteNfoParams.Rating)
            {
                bResult = true;
            }            

            return bResult;
        }
    }
}
