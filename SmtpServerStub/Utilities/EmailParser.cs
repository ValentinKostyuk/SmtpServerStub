﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using SmtpServerStub.Utilities.Interfaces;

namespace SmtpServerStub.Utilities
{
	internal class EmailParser : IEmailParser
	{
		private static readonly Regex MailAndNameRegex =
			new Regex(@"(?:\s*[""'](?<name>.+?)[""']\s*)?<?(?<address>[^<>]+)>?\s*",
				RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex MailAddressRegex =
			new Regex(@"<(?<address>[^<>]+?)>",
				RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex BodyStringPlainMessageRegex = new Regex("(?s)\\\r\\\n\\\r\\\n(?<body>.*)\\\r\\\n\\.\\\r\\\n",
			RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex HeadersParsingRegex = new Regex("(?s)(?<name>[A-Za-z -]+?):\\s*(?<value>.+?)(?=(?:\\\r\\\n[A-Za-z -]+?:)|(?:$))",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public IMailContentDecoder ContentDecoder { get; set; }

		public EmailParser()
		{
			ContentDecoder = new MailContentDecoder();
		}
		public virtual MailAddress ParseEmailFromString(string commandStr)
		{
			var match = MailAddressRegex.Match(commandStr);
			var address = match.Groups["address"].Value;
			return new MailAddress(address);
		}

		public virtual MailAddress ParseEmailFromEmailString(string commandStr)
		{
			var match = MailAndNameRegex.Match(commandStr);
			var address = match.Groups["address"].Value;
			var name = match.Groups["name"].Value;
			return new MailAddress(address, name);
		}

		public virtual List<MailAddress> ParseEmailsFromString(string commandStr)
		{
			if (commandStr == null)
			{
				return new List<MailAddress>();
			}

			var formattedString = commandStr.Trim().Replace("\n", string.Empty).Replace("\r", string.Empty);
			var splittedMailPairs = formattedString.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
			return splittedMailPairs.Select(ParseEmailFromEmailString).Distinct().ToList();
		}

		public virtual List<MailAddress> ParseEmailsFromDataCc(NameValueCollection headers)
		{
			var match = headers.Get("Cc");
			return ParseEmailsFromString(match);
		}

		public virtual List<MailAddress> ParseEmailsFromDataTo(NameValueCollection headers)
		{
			var match = headers.Get("To");
			return ParseEmailsFromString(match);
		}

		public virtual List<MailAddress> ParseEmailsFromDataFrom(NameValueCollection headers)
		{
			var match = headers.Get("From");
			return ParseEmailsFromString(match);
		}

		public virtual string ParseBodyFromDataSection(string dataSection)
		{
			if (DataSectionHasBoundaries(dataSection))
			{
				return GetBodyFromMessageWithAttachments(dataSection);
			}
			return GetBodyFromPlainEmail(dataSection);
		}

		public virtual string ParseSubjectFromDataSection(NameValueCollection headers)
		{
			return headers.Get("Subject");
		}

		public virtual bool GetIsMailBodyHtml(NameValueCollection headers)
		{
			var contentType = headers.Get("Content-Type");
			if (contentType == null)
			{
				return false;
			}
			return contentType.ToLowerInvariant().Contains("text/html");
		}

		public NameValueCollection ParseHeadersFromDataSection(string dataSection)
		{
			var result = new NameValueCollection();

			var indexOfSectionEnd = dataSection.IndexOf("\r\n\r\n", StringComparison.Ordinal);

			if (indexOfSectionEnd != -1)
			{
				var headersSection = dataSection.Substring(0, indexOfSectionEnd);
				var matches = HeadersParsingRegex.Matches(headersSection);
				foreach (Match match in matches)
				{
					result.Add(match.Groups["name"].Value.Trim(), match.Groups["value"].Value.Trim());
				}
			}

			return result;
		}

		private string GetBodyFromPlainEmail(string dataSection)
		{
			var headers = ParseHeadersFromDataSection(dataSection);
			var match = BodyStringPlainMessageRegex.Match(dataSection);
			var body = match.Groups["body"].Value?.Trim();

			var contentType = headers["Content-Type"];
			var contentTransferEncoding = headers["Content-Transfer-Encoding"];
			var decodedBody = ContentDecoder.DecodeContent(contentType, contentTransferEncoding, body);
			return decodedBody;
		}

		private string GetBodyFromMessageWithAttachments(string dataSection)
		{
			return "";
		}

		private bool DataSectionHasBoundaries(string dataSection)
		{
			return dataSection.Contains("boundary=");
		}
		
	}
}