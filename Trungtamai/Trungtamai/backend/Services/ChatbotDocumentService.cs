using System.IO.Compression;
using System.Xml.Linq;

namespace Trungtamai.Services
{
    public static class ChatbotDocumentService
    {
        private static readonly XNamespace PackageRelationshipNamespace =
            "http://schemas.openxmlformats.org/package/2006/relationships";
        private static readonly XNamespace ContentTypeNamespace =
            "http://schemas.openxmlformats.org/package/2006/content-types";
        private static readonly XNamespace WordNamespace =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private static readonly XNamespace SpreadsheetNamespace =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace OfficeRelationshipNamespace =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        public static byte[] CreateWordDocument(string content)
        {
            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
            {
                AddXmlEntry(archive, "[Content_Types].xml",
                    new XDocument(
                        new XElement(ContentTypeNamespace + "Types",
                            new XElement(ContentTypeNamespace + "Default",
                                new XAttribute("Extension", "rels"),
                                new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                            new XElement(ContentTypeNamespace + "Default",
                                new XAttribute("Extension", "xml"),
                                new XAttribute("ContentType", "application/xml")),
                            new XElement(ContentTypeNamespace + "Override",
                                new XAttribute("PartName", "/word/document.xml"),
                                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")),
                            new XElement(ContentTypeNamespace + "Override",
                                new XAttribute("PartName", "/word/styles.xml"),
                                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml")))));

                AddXmlEntry(archive, "_rels/.rels",
                    new XDocument(
                        new XElement(PackageRelationshipNamespace + "Relationships",
                            new XElement(PackageRelationshipNamespace + "Relationship",
                                new XAttribute("Id", "rId1"),
                                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                                new XAttribute("Target", "word/document.xml")))));

                AddXmlEntry(archive, "word/_rels/document.xml.rels",
                    new XDocument(
                        new XElement(PackageRelationshipNamespace + "Relationships",
                            new XElement(PackageRelationshipNamespace + "Relationship",
                                new XAttribute("Id", "rId1"),
                                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                                new XAttribute("Target", "styles.xml")))));

                AddXmlEntry(archive, "word/styles.xml", CreateWordStyles());
                var blocks = ParseWordBlocks(content);
                var body = blocks.SelectMany(CreateWordElements).ToList();
                body.Add(
                    new XElement(WordNamespace + "sectPr",
                        new XElement(WordNamespace + "pgSz",
                            new XAttribute(WordNamespace + "w", "12240"),
                            new XAttribute(WordNamespace + "h", "15840")),
                        new XElement(WordNamespace + "pgMar",
                            new XAttribute(WordNamespace + "top", "1080"),
                            new XAttribute(WordNamespace + "right", "1080"),
                            new XAttribute(WordNamespace + "bottom", "1080"),
                            new XAttribute(WordNamespace + "left", "1080"),
                            new XAttribute(WordNamespace + "header", "540"),
                            new XAttribute(WordNamespace + "footer", "540"),
                            new XAttribute(WordNamespace + "gutter", "0"))));

                AddXmlEntry(archive, "word/document.xml",
                    new XDocument(
                        new XElement(WordNamespace + "document",
                            new XAttribute(XNamespace.Xmlns + "w", WordNamespace),
                            new XElement(WordNamespace + "body", body))));
            }

            return output.ToArray();
        }

        public static byte[] CreateExcelDocument(string content)
        {
            var parsed = ParseSpreadsheetRows(content);
            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
            {
                AddXmlEntry(archive, "[Content_Types].xml",
                    new XDocument(
                        new XElement(ContentTypeNamespace + "Types",
                            new XElement(ContentTypeNamespace + "Default",
                                new XAttribute("Extension", "rels"),
                                new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                            new XElement(ContentTypeNamespace + "Default",
                                new XAttribute("Extension", "xml"),
                                new XAttribute("ContentType", "application/xml")),
                            new XElement(ContentTypeNamespace + "Override",
                                new XAttribute("PartName", "/xl/workbook.xml"),
                                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                            new XElement(ContentTypeNamespace + "Override",
                                new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
                            new XElement(ContentTypeNamespace + "Override",
                                new XAttribute("PartName", "/xl/styles.xml"),
                                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")))));

                AddXmlEntry(archive, "_rels/.rels",
                    new XDocument(
                        new XElement(PackageRelationshipNamespace + "Relationships",
                            new XElement(PackageRelationshipNamespace + "Relationship",
                                new XAttribute("Id", "rId1"),
                                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                                new XAttribute("Target", "xl/workbook.xml")))));

                AddXmlEntry(archive, "xl/workbook.xml",
                    new XDocument(
                        new XElement(SpreadsheetNamespace + "workbook",
                            new XAttribute(XNamespace.Xmlns + "r", OfficeRelationshipNamespace),
                            new XElement(SpreadsheetNamespace + "sheets",
                                new XElement(SpreadsheetNamespace + "sheet",
                                    new XAttribute("name", "Chatbot"),
                                    new XAttribute("sheetId", "1"),
                                    new XAttribute(OfficeRelationshipNamespace + "id", "rId1"))))));

                AddXmlEntry(archive, "xl/_rels/workbook.xml.rels",
                    new XDocument(
                        new XElement(PackageRelationshipNamespace + "Relationships",
                            new XElement(PackageRelationshipNamespace + "Relationship",
                                new XAttribute("Id", "rId1"),
                                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                                new XAttribute("Target", "worksheets/sheet1.xml")),
                            new XElement(PackageRelationshipNamespace + "Relationship",
                                new XAttribute("Id", "rId2"),
                                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                                new XAttribute("Target", "styles.xml")))));

                AddXmlEntry(archive, "xl/styles.xml", CreateExcelStyles());

                var maxColumns = Math.Max(1, parsed.Rows.Max(row => row.Count));
                var columns = new XElement(SpreadsheetNamespace + "cols",
                    Enumerable.Range(1, maxColumns).Select(column =>
                    {
                        var longest = parsed.Rows
                            .Where(row => row.Count >= column)
                            .Select(row => row[column - 1].Length)
                            .DefaultIfEmpty(10)
                            .Max();
                        return new XElement(SpreadsheetNamespace + "col",
                            new XAttribute("min", column),
                            new XAttribute("max", column),
                            new XAttribute("width", Math.Clamp(longest + 3, 14, 42)),
                            new XAttribute("customWidth", "1"));
                    }));

                var sheetRows = parsed.Rows.Select((row, rowIndex) =>
                {
                    var isHeader = rowIndex == parsed.HeaderRowIndex;
                    return new XElement(SpreadsheetNamespace + "row",
                        new XAttribute("r", rowIndex + 1),
                        new XAttribute("customHeight", "1"),
                        new XAttribute("ht", isHeader ? "30" : "24"),
                        row.Select((cell, columnIndex) =>
                            new XElement(SpreadsheetNamespace + "c",
                                new XAttribute("r", $"{GetColumnName(columnIndex + 1)}{rowIndex + 1}"),
                                new XAttribute("s", isHeader ? "1" : "2"),
                                new XAttribute("t", "inlineStr"),
                                new XElement(SpreadsheetNamespace + "is",
                                    new XElement(SpreadsheetNamespace + "t",
                                        new XAttribute(XNamespace.Xml + "space", "preserve"),
                                        cell)))));
                });

                var sheetData = new XElement(SpreadsheetNamespace + "sheetData", sheetRows);
                var worksheetElements = new List<object>
                {
                    new XElement(SpreadsheetNamespace + "sheetViews",
                        new XElement(SpreadsheetNamespace + "sheetView",
                            new XAttribute("workbookViewId", "0"),
                            new XElement(SpreadsheetNamespace + "pane",
                                new XAttribute("ySplit", "1"),
                                new XAttribute("topLeftCell", "A2"),
                                new XAttribute("activePane", "bottomLeft"),
                                new XAttribute("state", "frozen")),
                            new XElement(SpreadsheetNamespace + "selection",
                                new XAttribute("pane", "bottomLeft"),
                                new XAttribute("activeCell", "A2"),
                                new XAttribute("sqref", "A2")))),
                    new XElement(SpreadsheetNamespace + "sheetFormatPr",
                        new XAttribute("defaultRowHeight", "24")),
                    columns,
                    sheetData
                };

                if (parsed.HeaderRowIndex >= 0 && parsed.Rows.Count > parsed.HeaderRowIndex + 1)
                {
                    var lastCell = $"{GetColumnName(maxColumns)}{parsed.Rows.Count}";
                    worksheetElements.Add(new XElement(SpreadsheetNamespace + "autoFilter",
                        new XAttribute("ref", $"A{parsed.HeaderRowIndex + 1}:{lastCell}")));
                }

                worksheetElements.Add(new XElement(SpreadsheetNamespace + "pageMargins",
                    new XAttribute("left", "0.3"),
                    new XAttribute("right", "0.3"),
                    new XAttribute("top", "0.5"),
                    new XAttribute("bottom", "0.5"),
                    new XAttribute("header", "0.2"),
                    new XAttribute("footer", "0.2")));

                AddXmlEntry(archive, "xl/worksheets/sheet1.xml",
                    new XDocument(
                        new XElement(SpreadsheetNamespace + "worksheet", worksheetElements)));
            }

            return output.ToArray();
        }

        private static XDocument CreateWordStyles()
        {
            XElement ParagraphStyle(string id, string name, int size, string color, bool bold, int before, int after) =>
                new(WordNamespace + "style",
                    new XAttribute(WordNamespace + "type", "paragraph"),
                    new XAttribute(WordNamespace + "styleId", id),
                    new XElement(WordNamespace + "name", new XAttribute(WordNamespace + "val", name)),
                    id == "Normal" ? null : new XElement(WordNamespace + "basedOn", new XAttribute(WordNamespace + "val", "Normal")),
                    new XElement(WordNamespace + "next", new XAttribute(WordNamespace + "val", "Normal")),
                    new XElement(WordNamespace + "pPr",
                        new XElement(WordNamespace + "keepNext"),
                        new XElement(WordNamespace + "spacing",
                            new XAttribute(WordNamespace + "before", before),
                            new XAttribute(WordNamespace + "after", after))),
                    new XElement(WordNamespace + "rPr",
                        bold ? new XElement(WordNamespace + "b") : null,
                        new XElement(WordNamespace + "color", new XAttribute(WordNamespace + "val", color)),
                        new XElement(WordNamespace + "sz", new XAttribute(WordNamespace + "val", size)),
                        new XElement(WordNamespace + "szCs", new XAttribute(WordNamespace + "val", size))));

            return new XDocument(
                new XElement(WordNamespace + "styles",
                    new XAttribute(XNamespace.Xmlns + "w", WordNamespace),
                    new XElement(WordNamespace + "docDefaults",
                        new XElement(WordNamespace + "rPrDefault",
                            new XElement(WordNamespace + "rPr",
                                new XElement(WordNamespace + "rFonts",
                                    new XAttribute(WordNamespace + "ascii", "Aptos"),
                                    new XAttribute(WordNamespace + "hAnsi", "Aptos"),
                                    new XAttribute(WordNamespace + "eastAsia", "Aptos")),
                                new XElement(WordNamespace + "color", new XAttribute(WordNamespace + "val", "243247")),
                                new XElement(WordNamespace + "sz", new XAttribute(WordNamespace + "val", "22")))),
                        new XElement(WordNamespace + "pPrDefault",
                            new XElement(WordNamespace + "pPr",
                                new XElement(WordNamespace + "spacing",
                                    new XAttribute(WordNamespace + "after", "140"),
                                    new XAttribute(WordNamespace + "line", "300"),
                                    new XAttribute(WordNamespace + "lineRule", "auto"))))),
                    ParagraphStyle("Normal", "Normal", 22, "243247", false, 0, 140),
                    ParagraphStyle("Title", "Title", 36, "17365D", true, 0, 300),
                    ParagraphStyle("Heading1", "Heading 1", 30, "1F4E78", true, 300, 140),
                    ParagraphStyle("Heading2", "Heading 2", 25, "2F75B5", true, 220, 100)));
        }

        private static IEnumerable<XElement> CreateWordElements(WordBlock block)
        {
            if (block.IsTable)
            {
                var border = new XElement(WordNamespace + "tblBorders",
                    new[] { "top", "left", "bottom", "right", "insideH", "insideV" }
                        .Select(side => new XElement(WordNamespace + side,
                            new XAttribute(WordNamespace + "val", "single"),
                            new XAttribute(WordNamespace + "sz", "4"),
                            new XAttribute(WordNamespace + "space", "0"),
                            new XAttribute(WordNamespace + "color", "D6DEE8"))));
                var tableRows = new List<XElement>();
                for (var rowIndex = 0; rowIndex < block.Rows.Count; rowIndex++)
                {
                    var cells = new List<XElement>();
                    foreach (var cell in block.Rows[rowIndex])
                    {
                        var cellProperties = new List<object>
                        {
                            new XElement(WordNamespace + "tcMar",
                                new XElement(WordNamespace + "top",
                                    new XAttribute(WordNamespace + "w", "90"),
                                    new XAttribute(WordNamespace + "type", "dxa")),
                                new XElement(WordNamespace + "left",
                                    new XAttribute(WordNamespace + "w", "120"),
                                    new XAttribute(WordNamespace + "type", "dxa")),
                                new XElement(WordNamespace + "bottom",
                                    new XAttribute(WordNamespace + "w", "90"),
                                    new XAttribute(WordNamespace + "type", "dxa")),
                                new XElement(WordNamespace + "right",
                                    new XAttribute(WordNamespace + "w", "120"),
                                    new XAttribute(WordNamespace + "type", "dxa")))
                        };
                        if (rowIndex == 0)
                        {
                            cellProperties.Add(new XElement(WordNamespace + "shd",
                                new XAttribute(WordNamespace + "fill", "1F4E78"),
                                new XAttribute(WordNamespace + "val", "clear")));
                        }

                        var cellRun = new XElement(WordNamespace + "r",
                            rowIndex == 0
                                ? new XElement(WordNamespace + "rPr",
                                    new XElement(WordNamespace + "b"),
                                    new XElement(WordNamespace + "color",
                                        new XAttribute(WordNamespace + "val", "FFFFFF")))
                                : null,
                            new XElement(WordNamespace + "t",
                                new XAttribute(XNamespace.Xml + "space", "preserve"),
                                cell));

                        cells.Add(new XElement(WordNamespace + "tc",
                            new XElement(WordNamespace + "tcPr", cellProperties),
                            new XElement(WordNamespace + "p",
                                new XElement(WordNamespace + "pPr",
                                    new XElement(WordNamespace + "spacing",
                                        new XAttribute(WordNamespace + "after", "0"))),
                                cellRun)));
                    }

                    tableRows.Add(new XElement(WordNamespace + "tr", cells));
                }

                yield return new XElement(WordNamespace + "tbl",
                    new XElement(WordNamespace + "tblPr",
                        new XElement(WordNamespace + "tblW",
                            new XAttribute(WordNamespace + "w", "0"),
                            new XAttribute(WordNamespace + "type", "auto")),
                        new XElement(WordNamespace + "tblLayout", new XAttribute(WordNamespace + "type", "autofit")),
                        border),
                    tableRows);
                yield return new XElement(WordNamespace + "p",
                    new XElement(WordNamespace + "pPr",
                        new XElement(WordNamespace + "spacing",
                            new XAttribute(WordNamespace + "after", "100"))));
                yield break;
            }

            var styleId = block.Kind switch
            {
                "title" => "Title",
                "heading1" => "Heading1",
                "heading2" => "Heading2",
                _ => "Normal"
            };
            var paragraphText = block.Kind == "bullet" ? "•  " + block.Text : block.Text;
            yield return new XElement(WordNamespace + "p",
                new XElement(WordNamespace + "pPr",
                    new XElement(WordNamespace + "pStyle", new XAttribute(WordNamespace + "val", styleId)),
                    block.Kind == "bullet"
                        ? new XElement(WordNamespace + "ind",
                            new XAttribute(WordNamespace + "left", "360"),
                            new XAttribute(WordNamespace + "hanging", "180"))
                        : null),
                new XElement(WordNamespace + "r",
                    new XElement(WordNamespace + "t",
                        new XAttribute(XNamespace.Xml + "space", "preserve"),
                        paragraphText)));
        }

        private static List<WordBlock> ParseWordBlocks(string content)
        {
            var lines = content.ReplaceLineEndings("\n").Split('\n');
            var blocks = new List<WordBlock>();
            for (var index = 0; index < lines.Length;)
            {
                var line = lines[index].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    index++;
                    continue;
                }

                if (IsMarkdownTableRow(line))
                {
                    var rows = new List<List<string>>();
                    while (index < lines.Length && IsMarkdownTableRow(lines[index].Trim()))
                    {
                        var row = ParseMarkdownTableRow(lines[index].Trim());
                        if (!row.All(IsMarkdownTableSeparator))
                            rows.Add(row);
                        index++;
                    }

                    if (rows.Count > 0)
                        blocks.Add(WordBlock.Table(rows));
                    continue;
                }

                var normalized = NormalizeDocumentLine(line);
                var headingLevel = line.TakeWhile(character => character == '#').Count();
                var kind = headingLevel switch
                {
                    1 => "title",
                    2 => "heading1",
                    3 => "heading2",
                    _ when line.StartsWith("- ", StringComparison.Ordinal) ||
                           line.StartsWith("* ", StringComparison.Ordinal) ||
                           System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+[.)]\s+") => "bullet",
                    _ => "normal"
                };

                if (kind == "bullet" && System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^\d+[.)]\s+"))
                    normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"^\d+[.)]\s+", "");

                blocks.Add(WordBlock.Paragraph(normalized, kind));
                index++;
            }

            return blocks.Count == 0 ? [WordBlock.Paragraph("", "normal")] : blocks;
        }

        private static XDocument CreateExcelStyles()
        {
            return new XDocument(
                new XElement(SpreadsheetNamespace + "styleSheet",
                    new XElement(SpreadsheetNamespace + "fonts",
                        new XAttribute("count", "2"),
                        new XElement(SpreadsheetNamespace + "font",
                            new XElement(SpreadsheetNamespace + "sz", new XAttribute("val", "11")),
                            new XElement(SpreadsheetNamespace + "name", new XAttribute("val", "Aptos"))),
                        new XElement(SpreadsheetNamespace + "font",
                            new XElement(SpreadsheetNamespace + "b"),
                            new XElement(SpreadsheetNamespace + "color", new XAttribute("rgb", "FFFFFFFF")),
                            new XElement(SpreadsheetNamespace + "sz", new XAttribute("val", "11")),
                            new XElement(SpreadsheetNamespace + "name", new XAttribute("val", "Aptos")))),
                    new XElement(SpreadsheetNamespace + "fills",
                        new XAttribute("count", "3"),
                        new XElement(SpreadsheetNamespace + "fill",
                            new XElement(SpreadsheetNamespace + "patternFill", new XAttribute("patternType", "none"))),
                        new XElement(SpreadsheetNamespace + "fill",
                            new XElement(SpreadsheetNamespace + "patternFill", new XAttribute("patternType", "gray125"))),
                        new XElement(SpreadsheetNamespace + "fill",
                            new XElement(SpreadsheetNamespace + "patternFill",
                                new XAttribute("patternType", "solid"),
                                new XElement(SpreadsheetNamespace + "fgColor", new XAttribute("rgb", "FF1F4E78")),
                                new XElement(SpreadsheetNamespace + "bgColor", new XAttribute("indexed", "64"))))),
                    new XElement(SpreadsheetNamespace + "borders",
                        new XAttribute("count", "2"),
                        new XElement(SpreadsheetNamespace + "border",
                            new XElement(SpreadsheetNamespace + "left"),
                            new XElement(SpreadsheetNamespace + "right"),
                            new XElement(SpreadsheetNamespace + "top"),
                            new XElement(SpreadsheetNamespace + "bottom"),
                            new XElement(SpreadsheetNamespace + "diagonal")),
                        new XElement(SpreadsheetNamespace + "border",
                            new XElement(SpreadsheetNamespace + "left", new XAttribute("style", "thin"),
                                new XElement(SpreadsheetNamespace + "color", new XAttribute("rgb", "FFD6DEE8"))),
                            new XElement(SpreadsheetNamespace + "right", new XAttribute("style", "thin"),
                                new XElement(SpreadsheetNamespace + "color", new XAttribute("rgb", "FFD6DEE8"))),
                            new XElement(SpreadsheetNamespace + "top", new XAttribute("style", "thin"),
                                new XElement(SpreadsheetNamespace + "color", new XAttribute("rgb", "FFD6DEE8"))),
                            new XElement(SpreadsheetNamespace + "bottom", new XAttribute("style", "thin"),
                                new XElement(SpreadsheetNamespace + "color", new XAttribute("rgb", "FFD6DEE8"))),
                            new XElement(SpreadsheetNamespace + "diagonal"))),
                    new XElement(SpreadsheetNamespace + "cellStyleXfs",
                        new XAttribute("count", "1"),
                        new XElement(SpreadsheetNamespace + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "0"),
                            new XAttribute("fillId", "0"),
                            new XAttribute("borderId", "0"))),
                    new XElement(SpreadsheetNamespace + "cellXfs",
                        new XAttribute("count", "3"),
                        new XElement(SpreadsheetNamespace + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "0"),
                            new XAttribute("fillId", "0"),
                            new XAttribute("borderId", "0"),
                            new XAttribute("xfId", "0")),
                        new XElement(SpreadsheetNamespace + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "1"),
                            new XAttribute("fillId", "2"),
                            new XAttribute("borderId", "1"),
                            new XAttribute("xfId", "0"),
                            new XAttribute("applyFont", "1"),
                            new XAttribute("applyFill", "1"),
                            new XAttribute("applyBorder", "1"),
                            new XAttribute("applyAlignment", "1"),
                            new XElement(SpreadsheetNamespace + "alignment",
                                new XAttribute("horizontal", "center"),
                                new XAttribute("vertical", "center"),
                                new XAttribute("wrapText", "1"))),
                        new XElement(SpreadsheetNamespace + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "0"),
                            new XAttribute("fillId", "0"),
                            new XAttribute("borderId", "1"),
                            new XAttribute("xfId", "0"),
                            new XAttribute("applyBorder", "1"),
                            new XAttribute("applyAlignment", "1"),
                            new XElement(SpreadsheetNamespace + "alignment",
                                new XAttribute("vertical", "top"),
                                new XAttribute("wrapText", "1")))),
                    new XElement(SpreadsheetNamespace + "cellStyles",
                        new XAttribute("count", "1"),
                        new XElement(SpreadsheetNamespace + "cellStyle",
                            new XAttribute("name", "Normal"),
                            new XAttribute("xfId", "0"),
                            new XAttribute("builtinId", "0")))));
        }

        private static ParsedSpreadsheet ParseSpreadsheetRows(string content)
        {
            var rows = new List<List<string>>();
            var headerRowIndex = -1;
            foreach (var line in content.ReplaceLineEndings("\n").Split('\n'))
            {
                var trimmed = line.Trim();
                var delimiter = GetSpreadsheetDelimiter(trimmed);
                if (delimiter.HasValue)
                {
                    var cells = ParseDelimitedSpreadsheetRow(trimmed, delimiter.Value);
                    if (cells.All(IsMarkdownTableSeparator))
                        continue;
                    if (headerRowIndex < 0)
                        headerRowIndex = rows.Count;
                    rows.Add(cells);
                }
                else if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    var listSeparator = trimmed.IndexOf("Danh sách:", StringComparison.OrdinalIgnoreCase);
                    if (listSeparator >= 0)
                    {
                        var summary = trimmed[..listSeparator].Trim();
                        if (summary.Length > 0)
                            rows.Add([NormalizeDocumentLine(summary)]);

                        var listItems = trimmed[(listSeparator + "Danh sách:".Length)..]
                            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        rows.AddRange(listItems.Select(item => new List<string> { NormalizeDocumentLine(item) }));
                        continue;
                    }

                    rows.Add([NormalizeDocumentLine(trimmed)]);
                }
            }

            if (rows.Count == 0)
                rows.Add([""]);

            return new ParsedSpreadsheet(rows, headerRowIndex);
        }

        private static char? GetSpreadsheetDelimiter(string line)
        {
            if (line.Contains('\t'))
                return '\t';
            if (line.Contains('|'))
                return '|';
            if (line.Contains(';'))
                return ';';
            return null;
        }

        private static List<string> ParseDelimitedSpreadsheetRow(string line, char delimiter)
        {
            var row = delimiter == '|'
                ? line.Trim().Trim('|')
                : line.Trim();
            return row.Split(delimiter)
                .Select(cell => NormalizeDocumentLine(cell.Trim()))
                .ToList();
        }

        private static bool IsMarkdownTableRow(string line) =>
            line.StartsWith('|') && line.EndsWith('|') && line.Count(character => character == '|') >= 2;

        private static List<string> ParseMarkdownTableRow(string line) =>
            line.Trim('|')
                .Split('|')
                .Select(cell => NormalizeDocumentLine(cell.Trim()))
                .ToList();

        private static bool IsMarkdownTableSeparator(string cell)
        {
            var trimmed = cell.Trim().Trim(':');
            return trimmed.Length > 0 && trimmed.All(character => character == '-');
        }

        private static string NormalizeDocumentLine(string line)
        {
            var text = line.Trim();
            while (text.StartsWith('#'))
                text = text[1..].TrimStart();

            if (text.StartsWith("- ", StringComparison.Ordinal) ||
                text.StartsWith("* ", StringComparison.Ordinal))
                text = text[2..];
            else
                text = System.Text.RegularExpressions.Regex.Replace(text, @"^\d+[.)]\s+", "");

            return text.Replace("**", "", StringComparison.Ordinal)
                .Replace("__", "", StringComparison.Ordinal)
                .Replace("`", "", StringComparison.Ordinal);
        }

        private static void AddXmlEntry(ZipArchive archive, string name, XDocument document)
        {
            var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
            using var stream = entry.Open();
            document.Save(stream);
        }

        private static string GetColumnName(int columnNumber)
        {
            var name = "";
            while (columnNumber > 0)
            {
                columnNumber--;
                name = (char)('A' + columnNumber % 26) + name;
                columnNumber /= 26;
            }

            return name;
        }

        private sealed record ParsedSpreadsheet(List<List<string>> Rows, int HeaderRowIndex);

        private sealed record WordBlock(string Kind, string Text, List<List<string>> Rows)
        {
            public bool IsTable => Kind == "table";
            public static WordBlock Paragraph(string text, string kind) => new(kind, text, []);
            public static WordBlock Table(List<List<string>> rows) => new("table", "", rows);
        }
    }
}
