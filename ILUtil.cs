using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SmoothedController {
	static class ILUtil {
		public static bool CheckIL(IEnumerable<CodeInstruction> instructions, Dictionary<int, OpCode> confirmations) {
			foreach(var c in confirmations) {
				if(instructions.ElementAt(c.Key).opcode != c.Value)
					return false;
			}
			return true;
		}

		public static void InsertFn(int index, ref IEnumerable<CodeInstruction> instructions, MethodInfo fn) {
			if(fn.ReturnType != typeof(void) || !fn.IsStatic)
				throw new Exception("FN must be static void");

			var l = instructions.ToList();

			l.Insert(index, new CodeInstruction(OpCodes.Call, fn));

			instructions = l;
		}
	}
}
