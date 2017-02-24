using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Logical
{
	//~ type HTTPCodedError interface {
	//~ 	Error() string
	//~ 	Code() int
	//~ }
	public interface IHttpCodedError
	{
		string Error { get; }

		int Code { get; }
	}

	//~ type codedError struct {
	//~ 	s    string
	//~ 	code int
	//~ }
	public class CodedError : Exception, IHttpCodedError
	{
		public string Error
		{ get; private set; }

		public int Code
		{ get; private set; }

		//~ func (e *codedError) Error() string {
		//~ 	return e.s
		//~ }

		//~ func (e *codedError) Code() int {
		//~ 	return e.code
		//~ }

		//~ func CodedError(c int, s string) HTTPCodedError {
		//~ 	return &codedError{s, c}
		//~ }

		public CodedError(int c, string s) : base(s)
		{
			Code = c;
			Error = s;
		}
	}
}
