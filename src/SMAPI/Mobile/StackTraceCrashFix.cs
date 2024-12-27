using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace StardewModdingAPI.Mobile;

internal static class StackTraceCrashFix
{
    public static void Init(Harmony hp)
    {
        hp.Patch(
           original: AccessTools.FirstMethod(typeof(StackTrace), m => m.Name == "ToString" && m.GetParameters().Length == 2),
           prefix: new(typeof(StackTraceCrashFix), nameof(StackTrace_ToString3)),
           postfix: new(typeof(StackTraceCrashFix), nameof(Postfix_StackTrace_ToString3))
       );

        Console.WriteLine("Init StackTraceCrashFix");
    }
    private static bool ShowInStackTrace(MethodBase method)
    {
        Console.WriteLine("On ShowInStackTrace: method: " + method);
        if ((method.MethodImplementationFlags & MethodImplAttributes.AggressiveInlining) != 0)
        {
            Console.WriteLine("ShowInStackTrace return false 1");
            return false;
        }
        try
        {
            if (method.IsDefined(typeof(StackTraceHiddenAttribute), false))
            {
                Console.WriteLine("ShowInStackTrace return false 2");
                return false;
            }
            Type declaringType = method.DeclaringType;
            if (declaringType != null && declaringType.IsDefined(typeof(StackTraceHiddenAttribute), false))
            {
                Console.WriteLine("ShowInStackTrace return false 3");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error on ShowInStackTrace");
        }

        Console.WriteLine("ShowInStackTrace return true");
        return true;
    }

    static void My_StackTrace_ToString(StackTrace stack, object traceFormat, StringBuilder sb)
    {
        Console.WriteLine("On My Fix StackTrace.ToString(format, sb)");
        Console.WriteLine("trace format: " + traceFormat);

        int _numOfFrames = stack.FrameCount;
        if (1 == 0)
        {
        }
        string value = "at";
        if (1 == 0)
        {
        }
        string text = "in {0}:line {1}";
        if (1 == 0)
        {
        }
        string text2 = "in {0}:token 0x{1:x}+0x{2:x}";
        bool flag = true;
        for (int i = 0; i < _numOfFrames; i++)
        {
            Console.WriteLine($"On Frame: {i}");
            StackFrame frame = stack.GetFrame(i);
            MethodBase methodBase = frame?.GetMethod();
            Console.WriteLine($"method base: {methodBase}");

            if (!(methodBase != null) || (!ShowInStackTrace(methodBase) && i != _numOfFrames - 1))
            {
                Console.WriteLine("continue this frame");
                continue;
            }
            if (flag)
            {
                flag = false;
                Console.WriteLine("set flag to false");
            }
            else
            {
                sb.AppendLine();
                Console.WriteLine("new line empty");
            }

            sb.Append("   ").Append(value).Append(' ');
            bool flag2 = false;
            Type declaringType = methodBase.DeclaringType;
            string name = methodBase.Name;
            bool flag3 = false;
            //todo
            //if (declaringType != null && declaringType.IsDefined(typeof(CompilerGeneratedAttribute), false))
            //{
            //    flag2 = declaringType.IsAssignableTo(typeof(IAsyncStateMachine));
            //    if (flag2 || declaringType.IsAssignableTo(typeof(IEnumerator)))
            //    {
            //        flag3 = TryResolveStateMachineMethod(ref methodBase, out declaringType);
            //    }
            //}

            if (declaringType != null)
            {
                string fullName = declaringType.FullName;
                Console.WriteLine("try append on declaringType & full name");
                foreach (char c in fullName)
                {
                    sb.Append((c == '+') ? '.' : c);
                }
                sb.Append('.');
                Console.WriteLine("done append it");
            }
            sb.Append(methodBase.Name);
            Console.WriteLine("append method.Name");
            if (methodBase is MethodInfo { IsGenericMethod: not false } methodInfo)
            {
                Type[] genericArguments = methodInfo.GetGenericArguments();
                sb.Append('[');
                int k = 0;
                bool flag4 = true;
                for (; k < genericArguments.Length; k++)
                {
                    if (!flag4)
                    {
                        sb.Append(',');
                    }
                    else
                    {
                        flag4 = false;
                    }
                    sb.Append(genericArguments[k].Name);
                }
                sb.Append(']');
            }
            ParameterInfo[] array = null;
            Console.WriteLine("try get params to array");
            try
            {
                Console.WriteLine("try  methodBase.GetParameters()");
                //error here
                //fixme
                array = methodBase.GetParameters();
                Console.WriteLine("done get array");
            }
            catch
            {
            }
            finally
            {
                Console.WriteLine("finally methodBase.GetParameters()");
            }

            Console.WriteLine("params: " + array);
            if (array != null)
            {
                sb.Append('(');
                bool flag5 = true;
                for (int l = 0; l < array.Length; l++)
                {
                    if (!flag5)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        flag5 = false;
                    }
                    string value2 = "<UnknownType>";
                    if (array[l].ParameterType != null)
                    {
                        value2 = array[l].ParameterType.Name;
                    }
                    sb.Append(value2);
                    string name2 = array[l].Name;
                    if (name2 != null)
                    {
                        sb.Append(' ');
                        sb.Append(name2);
                    }
                }
                sb.Append(')');
            }
            Console.WriteLine("flag 3: " + flag3);
            if (flag3)
            {
                sb.Append('+');
                sb.Append(name);
                sb.Append('(').Append(')');
            }
            int ilOffset = frame.GetILOffset();
            Console.WriteLine("il offset: " + ilOffset);
            if (ilOffset != -1)
            {
                string fileName = frame.GetFileName();
                if (fileName != null)
                {
                    sb.Append(' ');
                    sb.AppendFormat(CultureInfo.InvariantCulture, text, fileName, frame.GetFileLineNumber());
                }
                //todo
                //else if (LocalAppContextSwitches.ShowILOffsets && methodBase.ReflectedType != null)
                //{
                //    string scopeName = methodBase.ReflectedType.Module.ScopeName;
                //    try
                //    {
                //        int metadataToken = methodBase.MetadataToken;
                //        P_1.Append(' ');
                //        P_1.AppendFormat(CultureInfo.InvariantCulture, text2, scopeName, metadataToken, frame.GetILOffset());
                //    }
                //    catch (InvalidOperationException)
                //    {
                //    }
                //}
            }
            var isLastFrame_FieldInfo = AccessTools.Field(
                typeof(StackFrame),
                "_isLastFrameFromForeignExceptionStackTrace");

            bool isLastFrame = (bool)isLastFrame_FieldInfo.GetValue(frame);
            Console.WriteLine("_isLastFrame: " + isLastFrame);
            if (isLastFrame && !flag2)
            {
                sb.AppendLine();
                if (1 == 0)
                {
                }
                sb.Append("--- End of stack trace from previous location ---");
            }
        }
        Console.WriteLine("trace format: " + traceFormat.ToString());
        if (traceFormat.ToString() == "TrailingNewLine")
        {
            sb.AppendLine();
        }

        Console.WriteLine("Done Fix My StackTrace.ToString(format, sb)");
    }
    static bool StackTrace_ToString3(StackTrace __instance, object traceFormat, ref StringBuilder sb)
    {
        Console.WriteLine("Prefix StackTrace_ToString3(format, stringBuilder)");
        var format = traceFormat;
        Console.WriteLine("format: " + format);
        Console.WriteLine("try call my StackTrace.ToString()");
        My_StackTrace_ToString(__instance, traceFormat, sb);
        if (sb.Length > 0)
        {
            Console.WriteLine("Fixed crash on StackTrace_ToString3");
            return false;
        }

        return true;
    }

    static void Postfix_StackTrace_ToString3(object traceFormat, StringBuilder sb)
    {
        Console.WriteLine("Postfix_StackTrace_ToString3(format, stringBuilder)");
        var format = traceFormat;
        Console.WriteLine("format: " + format);
    }

}
