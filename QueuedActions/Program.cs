using System;

namespace QueuedActions {
	public class Program {
		public static void Print(string message, int row = -1, int col = 0) {
			if (row >= 0 && col >= 0) {
				Console.SetCursorPosition(col, row);
			}
			Console.Write(message);
		}
		static void Main(string[] args) {
			ActionManager q = new ActionManager();
			KeyInput keyInput = new KeyInput();
			bool running = true;
			long now = Environment.TickCount;
			long then, soon;
			long deadmanSwitchStarted = Environment.TickCount;

			bool HasKeyPress() {
				if (keyInput.Count > 0) { Print("Got em!", 20); return true; }
				return false;
			}
			bool ProgramTimeout() => Environment.TickCount > deadmanSwitchStarted + 5000;
			bool EscapePressCheck() {
				Print("waiting for esc..." + Environment.TickCount, 21);
				if (keyInput.HasKey((char)27)) {
					Print("DONE                        ", 22);
					running = false;
					return true;
				}
				return false;
			}
			void RestartThisAction() {
				q.ResetAllActions();
			}

			q.Add(new ActionEntry {
				ID = "Quit On Escape",
				WhatToDo = () => { Print("\npress escape to quit", 22); },
				States = new ActionState[] { new ActionState("escapeKey", EscapePressCheck), }, LayerMask = 1
			});
			q.Add(new ActionEntry {
				ID = "wait", WaitTime = 3,
				WhatToDo = () => { Console.WriteLine("\nwaiting             "); }
			});
			q.Add(new ActionEntry {
				ID = "Wait for key press", WaitTime = 0.5f,
				WhatToDo = () => {
					Console.WriteLine("\npress key to continue (you have 5 seconds)");
					deadmanSwitchStarted = Environment.TickCount;
				},
				States = new ActionState[]{
					new ActionState("timeout", ProgramTimeout, () => running = false),
					new ActionState("success", HasKeyPress),
				}
			});
			bool paused = false;
			keyInput.BindKey(' ', () => paused = !paused);
			keyInput.BindKey('\b', RestartThisAction);
			while (running) {
				keyInput.Update();
				then = now;
				now = Environment.TickCount;
				float deltaTime = (now - then) / 1000f;
				if (!paused) {
					q.Update(deltaTime);
				}
				Console.SetCursorPosition(0, 0);
				q.PrintStuff();
				Console.WriteLine($"{q.WaitTime:0.00} {deltaTime:0.00}          \n");
				soon = now + 10;
				while (Environment.TickCount < soon && keyInput.Count == 0) {
					System.Threading.Thread.Sleep(1);
				}
			}
			Console.WriteLine("finished");
		}
	}
}
