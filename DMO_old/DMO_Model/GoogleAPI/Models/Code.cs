﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO_Model.GoogleAPI.Models
{
    /// <summary>
    /// <para>The canonical error codes for Google APIs.</para>
    /// <para>Sometimes multiple error codes may apply.  Services should return
    /// the most specific error code that applies.  For example, prefer
    /// `OUT_OF_RANGE` over `FAILED_PRECONDITION` if both codes apply.
    /// Similarly prefer `NOT_FOUND` or `ALREADY_EXISTS` over `FAILED_PRECONDITION`.</para>
    /// </summary>
    public enum Code
    {
        /// <summary>
        /// <para>Not an error; returned on success</para>
        /// <para>HTTP Mapping: 200 OK</para>
        /// </summary>
        OK = 0,
        
        /// <summary>
        /// <para>The operation was cancelled, typically by the caller.</para>
        /// <para>HTTP Mapping: 499 Client Closed Request</para>
        /// </summary>
        CANCELLED = 1,
        
        /// <summary>
        /// <para>Unknown error.  For example, this error may be returned when
        /// a `Status` value received from another address space belongs to
        /// an error space that is not known in this address space.  Also
        /// errors raised by APIs that do not return enough error information
        /// may be converted to this error.</para>
        /// <para>HTTP Mapping: 500 Internal Server Error</para>
        /// </summary>
        UNKNOWN = 2,
        
        /// <summary>
        /// <para>The client specified an invalid argument.  Note that this differs
        /// from `FAILED_PRECONDITION`.  `INVALID_ARGUMENT` indicates arguments
        /// that are problematic regardless of the state of the system
        /// (e.g., a malformed file name).</para>
        /// <para>HTTP Mapping: 400 Bad Request</para>
        /// </summary>
        INVALID_ARGUMENT = 3,
        
        /// <summary>
        /// <para>The deadline expired before the operation could complete. For operations
        /// that change the state of the system, this error may be returned
        /// even if the operation has completed successfully.  For example, a
        /// successful response from a server could have been delayed long
        /// enough for the deadline to expire.</para>
        /// <para>HTTP Mapping: 504 Gateway Timeout</para>
        /// </summary>
        DEADLINE_EXCEEDED = 4,
        
        /// <summary>
        /// <para>Some requested entity (e.g., file or directory) was not found.</para>
        /// <para>Note to server developers: if a request is denied for an entire class
        /// of users, such as gradual feature rollout or undocumented whitelist,
        /// `NOT_FOUND` may be used. If a request is denied for some users within
        /// a class of users, such as user-based access control, `PERMISSION_DENIED`
        /// must be used.</para>
        /// <para>HTTP Mapping: 404 Not Found</para>
        /// </summary>
        NOT_FOUND = 5,
        
        /// <summary>
        /// <para>The entity that a client attempted to create (e.g., file or directory)
        /// already exists.</para>
        /// <para>HTTP Mapping: 409 Conflict</para>
        /// </summary>
        ALREADY_EXISTS = 6,
        
        /// <summary>
        /// <para>The caller does not have permission to execute the specified
        /// operation. `PERMISSION_DENIED` must not be used for rejections
        /// caused by exhausting some resource (use `RESOURCE_EXHAUSTED`
        /// instead for those errors). `PERMISSION_DENIED` must not be
        /// used if the caller can not be identified (use `UNAUTHENTICATED`
        /// instead for those errors). This error code does not imply the
        /// request is valid or the requested entity exists or satisfies
        /// other pre-conditions.</para>
        /// <para>HTTP Mapping: 403 Forbidden</para>
        /// </summary>
        PERMISSION_DENIED = 7,
        
        /// <summary>
        /// <para>The request does not have valid authentication credentials for the
        /// operation.</para>
        /// <para>HTTP Mapping: 401 Unauthorized</para>
        /// </summary>
        UNAUTHENTICATED = 16,
        
        /// <summary>
        /// <para>Some resource has been exhausted, perhaps a per-user quota, or
        /// perhaps the entire file system is out of space.</para>
        /// <para>HTTP Mapping: 429 Too Many Requests</para>
        /// </summary>
        RESOURCE_EXHAUSTED = 8,

        /// <summary>
        /// <para>The operation was rejected because the system is not in a state
        /// required for the operation's execution.  For example, the directory
        /// to be deleted is non-empty, an rmdir operation is applied to
        /// a non-directory, etc.</para>
        /// <para>Service implementors can use the following guidelines to decide
        /// between `FAILED_PRECONDITION`, `ABORTED`, and `UNAVAILABLE`:</para>
        /// <para>Use `UNAVAILABLE` if the client can retry just the failing call.</para>
        /// <para>Use `ABORTED` if the client should retry at a higher level
        ///      (e.g., when a client-specified test-and-set fails, indicating the
        ///      client should restart a read-modify-write sequence).</para>
        /// <para>Use `FAILED_PRECONDITION` if the client should not retry until
        ///      the system state has been explicitly fixed.  E.g., if an "rmdir"
        ///      fails because the directory is non-empty, `FAILED_PRECONDITION`
        ///      should be returned since the client should not retry unless
        ///      the files are deleted from the directory.</para>
        /// <para>HTTP Mapping: 400 Bad Request</para>
        /// </summary>
        FAILED_PRECONDITION = 9,
        
        /// <summary>
        /// <para>The operation was aborted, typically due to a concurrency issue such as
        /// a sequencer check failure or transaction abort.</para>
        /// <para>See the guidelines above for deciding between `FAILED_PRECONDITION`,
        /// `ABORTED`, and `UNAVAILABLE`.</para>
        /// <para>HTTP Mapping: 409 Conflict</para>
        /// </summary>
        ABORTED = 10,
        
        /// <summary>
        /// <para>The operation was attempted past the valid range.  E.g., seeking or
        /// reading past end-of-file.</para>
        /// <para>Unlike `INVALID_ARGUMENT`, this error indicates a problem that may
        /// be fixed if the system state changes. For example, a 32-bit file
        /// system will generate `INVALID_ARGUMENT` if asked to read at an
        /// offset that is not in the range [0,2^32-1], but it will generate
        /// `OUT_OF_RANGE` if asked to read from an offset past the current
        /// file size.</para>
        /// <para>There is a fair bit of overlap between `FAILED_PRECONDITION` and
        /// `OUT_OF_RANGE`.  We recommend using `OUT_OF_RANGE` (the more specific
        /// error) when it applies so that callers who are iterating through
        /// a space can easily look for an `OUT_OF_RANGE` error to detect when
        /// they are done.</para>
        /// <para>HTTP Mapping: 400 Bad Request</para>
        /// </summary>
        OUT_OF_RANGE = 11,

        // 
        //
        // 
        /// <summary>
        /// <para>The operation is not implemented or is not supported/enabled in this
        /// service.</para>
        /// <para>HTTP Mapping: 501 Not Implemented</para>
        /// </summary>
        UNIMPLEMENTED = 12,
        
        /// <summary>
        /// <para>Internal errors.  This means that some invariants expected by the
        /// underlying system have been broken.  This error code is reserved
        /// for serious errors.</para>
        /// <para>HTTP Mapping: 500 Internal Server Error</para>
        /// </summary>
        INTERNAL = 13,
        
        /// <summary>
        /// <para>The service is currently unavailable.  This is most likely a
        /// transient condition, which can be corrected by retrying with
        /// a backoff.</para>
        /// <para>See the guidelines above for deciding between `FAILED_PRECONDITION`,
        /// `ABORTED`, and `UNAVAILABLE`.</para>
        /// <para>HTTP Mapping: 503 Service Unavailable</para>
        /// </summary>
        UNAVAILABLE = 14,
        
        /// <summary>
        /// <para>Unrecoverable data loss or corruption.</para>
        /// <para>HTTP Mapping: 500 Internal Server Error</para>
        /// </summary>
        DATA_LOSS = 15
    }
}
