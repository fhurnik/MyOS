using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Modules.Notes.Application.Errors
{
    public sealed class TextNoteErrors : ErrorCodes
    {
        private TextNoteErrors() { } // reflection only — see ErrorCodes base class

        public static readonly Error NotFound =
            Error.NotFound("TextNoteErrors.NotFound");

        public static readonly Error Forbidden =
            Error.Forbidden("TextNoteErrors.Forbidden");
    }
}
