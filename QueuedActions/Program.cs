using System;

namespace QueuedActions {
	public class Program {
		public static long current_milli_time() {
			return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}
		public static void Print(string message, int row = -1, int col = 0) {
			if (row >= 0 && col >= 0) {
				Console.SetCursorPosition(col, row);
			}
			Console.Write(message);
		}
		static void Main(string[] args) {
			action_manager q = new action_manager();
			key_input keyInput = new key_input();
			bool running = true;
			long now = current_milli_time();
			long then, soon;
			long deadmanSwitchStarted = current_milli_time();
			int messageIndex = 0;
			string message = "This is a message";

			bool HasKeyPress(action_entry_record record) {
				if (keyInput.count() > 0) { Print("Got em!\n", 20); return true; }
				return false;
			}
			bool ProgramTimeout(action_entry_record record) => current_milli_time() > deadmanSwitchStarted + 5000;
			bool EscapePressCheck(action_entry_record record) {
				//Print(".");
				if (keyInput.has_key((char)27)) {
					Print("DONE\n", 22);
					running = false;
					return true;
				}
				return false;
			}
			void RestartThisAction() {
				q.reset_all_actions();
			}

			action_entry wait2SecondsWithMessage = new action_entry {
				id = "wait", wait_time = 2,
				what_to_do = () => { Print("waiting\n", 20); }
			};
			action_entry fastWait = new action_entry { id = "wait", wait_time = 0.125f };
			action_entry printMessageLetterAndIncrement = new action_entry {
				id = "printMessage", what_to_do = () => {
					Console.ForegroundColor = ConsoleColor.Green;
					Print(message[messageIndex].ToString(), 19, messageIndex++);
					Console.ForegroundColor= ConsoleColor.Gray;
				}, on_reset = (r) => { messageIndex = 0; }
			};
			q.append(new action_entry {
				id = "Quit On Escape",
				what_to_do = () => { Print("press escape to quit\n", 22); },
				conditionals = new action_entry_conditional[] { new action_entry_conditional("escapeKey", EscapePressCheck), }, blocking_layer_mask = 1
			});
			q.append(wait2SecondsWithMessage);
			for (int i = 0; i < message.Length; i++) {
				q.append(printMessageLetterAndIncrement);
				q.append(fastWait);
			}
			q.append(new action_entry {
				id = "Wait for key press before quit",
				what_to_do = () => {
					Print("press key to continue (you have 5 seconds)\n", 23);
					deadmanSwitchStarted = current_milli_time();
				},
				conditionals = new action_entry_conditional[] {
					new action_entry_conditional("timeout", ProgramTimeout, (r) => running = false),
					new action_entry_conditional("success", HasKeyPress, (r) => Print("(success)\n", 23)),
				}
			});
			bool paused = false;
			//keyInput.bind_key(' ', () => paused = !paused);
			keyInput.bind_key('\b', RestartThisAction);
			while (running) {
				keyInput.update();
				then = now;
				now = current_milli_time();
				float deltaTime = (now - then) / 1000f;
				if (!paused) {
					if (q.update(deltaTime)) {
						Print("", 0, 0);
						q.print_stuff();
					}
					if (q.wait_time() > 0) {
						Print("", 10, 0);
						Console.WriteLine($"{q.wait_time():0.00} {deltaTime:0.00}");
					}
				}
				//Console.SetCursorPosition(0, 0);
				soon = now + 100;
				while (current_milli_time() < soon && keyInput.count() == 0) {
					System.Threading.Thread.Sleep(1);
				}
			}
			Console.WriteLine("finished");
		}
	}
}
