﻿//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Vulcain.Core.Log
//{

//    string EntryKind = "RR"  // receive request
//    | "Log"     // normal log
//    | "RR"      // Receive request
//    | "ER"      // end request
//    | "RT"      // Receive task
//    | "ET"      // End task
//    | "BC"      // begin command
//    | "EC"      // end command
//    | "RE"      // Receive event
//    | "EE"      // end event
//    ;

//interface LogEntry
//    {
//        correlationId: string;
//    parentId: string;
//    traceId: string;
//    service: string;
//    version: string;
//    source: string; // container
//    message?: string;
//    timestamp: number;
//    kind: EntryKind;
//    error?: string;
//    stack?: string;
//}

//    public class VulcainLogger : Logger
//    {

//    private static _enableInfo: IDynamicProperty<boolean>;
//    private _hostname: string;

//    private static get enableInfo()
//    {
//        if (!VulcainLogger._enableInfo)
//            VulcainLogger._enableInfo = System && DynamicConfiguration.getChainedConfigurationProperty("enableVerboseLog", false);
//        return VulcainLogger._enableInfo.value;
//    }

//    constructor()
//    {
//        this._hostname = os.hostname();
//    }

//    /**
//     * Log an error
//     *
//     * @param {any} context Current context
//     * @param {Error} error Error instance
//     * @param {string} [msg] Additional message
//     *
//     * @memberOf VulcainLogger
//     */
//    error(context: IRequestContext|null, error: Error, msg?: ()=>string)
//    {
//        if (!error) return;
//        let entry = this.prepareEntry(context);
//        entry.message = (msg && msg()) || "Error occured";
//        entry.error = error.message;
//        if (!(error instanceof ApplicationError))
//            entry.stack = (error.stack || "").replace(/[\r\n] / g, '↵');

//        this.writeEntry(entry);
//    }

//    /**
//     * Log a message
//     *
//     * @param {any} context Current context
//     * @param {string} msg Message format (can include %s, %j ...)
//     * @param {...Array<string>} params Message parameters
//     *
//     * @memberOf VulcainLogger
//     */
//    info(context: IRequestContext|null, msg: ()=>string)
//    {
//        let entry = this.prepareEntry(context);
//        entry.message = msg && msg();
//        this.writeEntry(entry);
//    }

//    /**
//     * Log a verbose message. Verbose message are enable by service configuration property : enableVerboseLog
//     *
//     * @param {any} context Current context
//     * @param {string} msg Message format (can include %s, %j ...)
//     * @param {...Array<string>} params Message parameters
//     *
//     * @memberOf VulcainLogger
//     */
//    verbose(context: IRequestContext|null, msg: ()=>string)
//    {
//        if (VulcainLogger.enableInfo || System.isDevelopment)
//        {
//            this.info(context, msg);
//            return true;
//        }
//        return false;
//    }

//    logAction(context: IRequestContext, kind: EntryKind, message?: string)
//    {
//        let entry = this.prepareEntry(context);
//        entry.kind = kind;
//        entry.message = message;
//        this.writeEntry(entry);
//    }

//    private prepareEntry(context: IRequestContext|null)
//    {
//        let trackerId: TrackerId;
//        let ctx = context;
//        while (ctx)
//        {
//            if (ctx.tracker)
//                trackerId = ctx.tracker.id;
//            ctx = ctx.parent;
//        }

//        if (System.isDevelopment)
//        {
//            return < LogEntry >{
//                correlationId: (trackerId && trackerId.correlationId) || undefined,
//                parentId: (trackerId && trackerId.parentId) || undefined,
//                traceId: (trackerId && trackerId.spanId) || undefined
//              };
//        }

//        return < LogEntry >{
//            service: System.serviceName,
//            version: System.serviceVersion,
//            kind: "Log",
//            source: this._hostname,
//            timestamp: Date.now() * 1000, // TODO
//            correlationId: (trackerId && trackerId.correlationId) || undefined,
//            parentId: (trackerId && trackerId.parentId) || undefined,
//            traceId: (trackerId && trackerId.spanId) || undefined
//          };
//    }

//    private writeEntry(entry: LogEntry)
//    {

//        if (System.isDevelopment)
//        {
//            if (entry.kind === "RR")
//                console.log("======================");
//            const msg = entry.message;
//            entry.message = undefined;
//            let str = JSON.stringify(entry);
//            str = str.substr(1, str.length - 2); // Remove {}
//            if (str)
//                str = "- " + str;
//            let timestamp = ms().format('HH:mm:ss:SSS');
//            console.log(`${ timestamp}
//            - ${ msg} ${ str}`);
//        }
//        else
//        {
//            console.log(JSON.stringify(entry));
//        }
//    }
//}

//}
