/*
 * Copyright (c) 2005 Mike Bridge <mike@bridgecanada.com>
 * 
 * Permission is hereby granted, free of charge, to any 
 * person obtaining a copy of this software and associated 
 * documentation files (the "Software"), to deal in the 
 * Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, 
 * distribute, sublicense, and/or sell copies of the 
 * Software, and to permit persons to whom the Software 
 * is furnished to do so, subject to the following 
 * conditions:
 *
 * The above copyright notice and this permission notice 
 * shall be included in all copies or substantial portions 
 * of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF 
 * ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT 
 * SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
 * OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;

namespace DotNetOpenMail
{
	/// <summary>
	/// SMTP server response codes and their meanings.
	/// </summary>
	public enum SmtpResponseCode: int
	{
		/// <remarks>
		/// System status, or system help reply.
		/// </remarks>
		SystemStatus = 211,
		/// <remarks>
		/// Help message. 
		/// </remarks>
		Help = 214,					
		/// <remarks>
		/// Domain service ready. Ready to start TLS.
		/// </remarks>
		HeloReply = 220,
		/// <remarks>
		/// Domain service closing transmission channel.
		/// </remarks>
		Quit = 221,
		/// <remarks>
		/// Authentication successfully completed.
		/// </remarks>
		AuthSuccessful = 235,
		/// <remarks>
		/// OK, queuing for node node started. Requested mail action completed.
		/// </remarks>
		Ok = 250,
		/// <remarks>
		/// OK, no messages waiting for node node. User not local, will forward to forwardpath.
		/// </remarks>
		OkWillForward = 251,
		/// <remarks>
		/// OK, pending messages for node node started. Cannot VRFY user (e.g., info is not local), but will take message for this user and attempt delivery.
		/// </remarks>
		OkWithoutVerify = 252,
		/// <remarks>
		/// OK, messages pending messages for node node started.
		/// </remarks>
		OkMsgStarted = 253,
		/// <remarks>
		/// Start mail input, end with CRLF.CRLF
		/// </remarks>
		StartMailInput = 354,
		/// <remarks>
		/// Octet-offset is the transaction offset.
		/// </remarks>
		TransactionOffset = 355,
		/// <remarks>
		/// Domain service not available, closing transmission channel.
		/// </remarks>
		ServiceNotAvailable = 421,
		/// <remarks>
		/// A password transition is needed.
		/// </remarks>
		PasswordNeeded = 432,
		/// <remarks>
		/// Requested mail action not taken: mailbox unavailable.
		/// </remarks>
		MailboxBusy = 450,
		/// <remarks>
		/// Requested action aborted: local error in processing. Unable to process ATRN request now.
		/// </remarks>
		ErrorProcessing = 451,
		/// <remarks>
		/// Requested action not taken: insufficient system storage.
		/// </remarks>
		InsufficientStorage = 452,
		/// <remarks>
		/// You have no mail.
		/// </remarks>
		NoMail = 453,
		/// <remarks>
		/// TLS not available. Encryption required for requested authentication mechanism.
		/// </remarks>
		TlsNotAvailable = 454,
		/// <remarks>
		/// Unable to queue messages for node node.
		/// </remarks>
		NoMsgQueue = 458,
		/// <remarks>
		/// Node node not allowed: reason.
		/// </remarks>
		NodeNotAllowed = 459,
		/// <remarks>
		/// Command not recognized: command. Syntax error.
		/// </remarks>
		UnknownCmd = 500,
		/// <remarks>
		/// Syntax error, no parameters allowed.
		/// </remarks>
		SyntaxError = 501,
		/// <remarks>
		/// Command not implemented.
		/// </remarks>
		CmdNotImplemented = 502,
		/// <remarks>
		/// Bad sequence of commands.
		/// </remarks>
		BadSequence = 503,
		/// <remarks>
		/// Command parameter not implemented.
		/// </remarks>
		ParamNotImplemented = 504,
		/// <remarks>
		/// Security error.
		/// </remarks>
		SecurityError = 505,
		/// <remarks>
		/// Machine does not accept mail.
		/// </remarks>
		MailNotAccepted = 521,
		/// <remarks>
		/// Must issue a STARTTLS command first. Encryption required for requested authentication mechanism.
		/// </remarks>
		StartTLSneeded = 530,
		/// <remarks>
		/// Authentication mechanism is too weak.
		/// </remarks>
		AuthTooWeak = 534,
		/// <remarks>
		/// Encryption required for requested authentication mechanism.
		/// </remarks>
		EncryptionRequired = 538,
		/// <remarks>
		/// Requested action not taken: mailbox unavailable.
		/// </remarks>
		ActionNotTaken = 550,
		/// <remarks>
		/// User not local, please try forward path.
		/// </remarks>
		NotLocalPleaseForward = 551,
		/// <remarks>
		/// Requested mail action aborted: exceeded storage allocation.
		/// </remarks>
		ExceedStorageAllowance = 552,
		/// <remarks>
		/// Requested action not taken: mailbox name not allowed.
		/// </remarks>
		MailboxNameNotAllowed = 553,
		/// <remarks>
		/// Transaction failed.
		/// </remarks>
		TransactionFailed = 554
	}
}
