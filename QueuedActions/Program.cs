using System;
//py import time
//py from action_manager import *
//py from key_input import *
//py from colorama import init as colorama_init
//py from colorama import Fore
//py from colorama import Back
//py from colorama import Style
//py colorama_init()
namespace QueuedActions {
  //py def current_milli_time():
    //py return time.time() * 1000
  //py def sleepMs(ms):
    //py time.sleep(ms/1000)
  //py def Print(message, row = -1, col = 0):
    //py print(message, end = "")
  //py def move_cursor (row, col): # -> void
    //py #print(f"\033[{row};{col}H", end= "", flush= True)
    //py pass
  public class Program {
    /*#*/public static long current_milli_time() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    /*#*/public static void move_cursor(int row, int col) => Console.SetCursorPosition(col, row);
    /*#*/public static string str(object o) => o.ToString();
    /*#*/public static int len(string s) => s.Length;
    /*#*/public static void Print(/*this,*/string message, int row = -1, int col = 0) {
    //py def Print(this, message, row = -1, col = 0):
      if (row >= 0 && col >= 0) {
        //move_cursor(row, col);
        //py pass
      }
      bool coloredToken = false;
      char lastChar = ' ';
      for (int i = 0; i < len(message); i++) {
        char c = message[i];
        if (coloredToken && c == ' ') {
          /*#*/Console.ForegroundColor = ConsoleColor.Gray; Console.BackgroundColor = ConsoleColor.Black;
          //py print(f"{Fore.WHITE}{Back.BLACK}")
          coloredToken = false;
        } else if (!coloredToken && lastChar == ' ') {
          if (c == '>') {
            /*#*/Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.DarkGray;
            //py print(f"{Fore.LIGHTWHITE_EX}{Back.LIGHTBLACK_EX}")
            coloredToken = true;
          } else if (c == '-') {
            /*#*/Console.ForegroundColor = ConsoleColor.DarkGray; Console.BackgroundColor = ConsoleColor.Black;
            //py print(f"{Fore.LIGHTBLACK_EX}{Back.BLACK}")
            coloredToken = true;
          } else if (c == '.') {
            /*#*/Console.ForegroundColor = ConsoleColor.DarkGreen; Console.BackgroundColor = ConsoleColor.Black;
            //py print(f"{Fore.GREEN}{Back.BLACK}")
            coloredToken = true;
          } else if (c == '!') {
            /*#*/Console.ForegroundColor = ConsoleColor.Green; Console.BackgroundColor = ConsoleColor.Black;
            //py print(f"{Fore.LIGHTGREEN_EX}{Back.BLACK}")
            coloredToken = true;
          } else if (c == 'X') {
            /*#*/Console.ForegroundColor = ConsoleColor.Red; Console.BackgroundColor = ConsoleColor.Black;
            //py print(f"{Fore.LIGHTRED_EX}{Back.BLACK}")
            coloredToken = true;
          }
        }
        //py print(message, end = "")
        /*#*/Console.Write(c);
        lastChar = c;
      }
    }
    /*#*/public static void sleepMs(int ms) {
      /*#*/System.Threading.Thread.Sleep(ms);
    /*#*/}
    //py def main():
    /*#*/public static void Main(string[] args) {
      //py global running, paused, deadmanSwitchStarted, messageIndex
      action_manager q = new action_manager();
      key_input keyInput = new key_input();
      bool running = true;
      long now = current_milli_time();
      long then = now;
      long soon = now;
      long deadmanSwitchStarted = now;
      int messageIndex = 0;
      string message = "This is a message";
      bool paused = false;

      bool HasKeyPress(object context) {
        if (keyInput.count() > 0) {
          Print("Got em!\n", 20);
          return true;
        }
        return false;
      }
      void DoPrintSuccess(object context) {
        Print("(success)\n", 23);
      }
      void DoBreakMainLoop(object context) {
        //py global running
        running = false;
      }
      bool IsProgramTimeout(object context) {
        //py global deadmanSwitchStarted
        return current_milli_time() > deadmanSwitchStarted + 5000;
      }
      bool IsEscapePressed(object context) {
        /*#*/bool trigger =  keyInput.has_key((char)27);
        //py trigger = keyInput.has_key('esc');
        return trigger;
      }
      void RestartThisAction(object context) {
        q.reset_all_actions();
      }
      void PrintWaiting(object context) {
        Print("waiting\n", 20);
      }
      void PrintOneLetterOfMessage(object context) {
        /*#*/Console.ForegroundColor = ConsoleColor.Green;
        //py global messageIndex
        Print(str(message[messageIndex]), 19, messageIndex);
        messageIndex += 1;
        /*#*/Console.ForegroundColor = ConsoleColor.Gray;
      }
      void ResetMessagePrinting(object context) {
        //py global messageIndex
        messageIndex = 0;
        /*#*/string clearMessage = new String(' ', len(message));
        //py clearMessage = ' ' * len(message)
        Print(clearMessage, 19, 0);
      }
      void RestartDeadmanSwitch(object context) {
        //py global deadmanSwitchStarted
        Print("press key to continue (you have 5 seconds)\n", 23);
        deadmanSwitchStarted = current_milli_time();
      }
      void TogglePause(object context) {
        //py global paused
        paused = !paused;
      }
      action_entry wait2SecondsWithMessage = new action_entry("wait2sec");
      wait2SecondsWithMessage.wait_time = 2;
      wait2SecondsWithMessage.what_to_do = PrintWaiting;
      action_entry fastWait = new action_entry("<1s");
      fastWait.wait_time = 0.125f;
      /*#*/fastWait.conditionals = new action_entry_conditional[] {
      //py fastWait.conditionals = [
        new action_entry_conditional("waitFinished", null, null),
        new action_entry_conditional("interrupted", HasKeyPress, TogglePause),
      //py ]
      };
      action_entry printMessageLetterAndIncrement = new action_entry("printPartial");
      printMessageLetterAndIncrement.what_to_do = PrintOneLetterOfMessage;
      printMessageLetterAndIncrement.on_reset = ResetMessagePrinting;
      action_entry quitOnEscape = new action_entry("QuitOnEsc");
      /*#*/quitOnEscape.conditionals = new action_entry_conditional[] {
      //py quitOnEscape.conditionals = [
        new action_entry_conditional("escapeKey", IsEscapePressed, DoBreakMainLoop)
      //py ]
      };
      quitOnEscape.blocking_layer_mask = 1;
      action_entry waitForKeyPressBeforeQuit = new action_entry("WaitB4quit");
      waitForKeyPressBeforeQuit.what_to_do = RestartDeadmanSwitch;
      /*#*/waitForKeyPressBeforeQuit.conditionals = new action_entry_conditional[] {
      //py waitForKeyPressBeforeQuit.conditionals = [
        new action_entry_conditional("success", HasKeyPress, DoPrintSuccess),
        new action_entry_conditional("timeout", IsProgramTimeout, DoBreakMainLoop),
      //py ]
      };
      q.append(quitOnEscape);
      q.append(wait2SecondsWithMessage);
      for (int i = 0; i < len(message); i++) {
        q.append(printMessageLetterAndIncrement);
        q.append(fastWait);
      }
      q.append(waitForKeyPressBeforeQuit);

      keyInput.bind_key('p', TogglePause);
      keyInput.bind_key('r', RestartThisAction);
      while (running) {
        keyInput.update();
        then = now;
        now = current_milli_time();
        float deltaTime = (now - then) / 1000f;
        if (!paused) {
          if (q.update(deltaTime)) {
            Print(q.print_stuff()+"\n", 0, 0);
          }
          if (q.wait_time() > 0) {
            /*#*/Print($"{q.wait_time():0.00} {deltaTime:0.00}\n", 10, 0);
            //py Print($"{q.wait_time():.2f} {deltaTime:.2f}\n", 10, 0);
          }
        }
        soon = now + 100;
        while (current_milli_time() < soon && keyInput.count() == 0) {
          sleepMs(1);
        }
      }
      Print("finished");
    }
    //py if __name__ == "__main__":
      //py main()
  }
}
