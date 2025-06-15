using System;
using System.Collections;
using System.Collections.Generic;
//py from action_entry import *
//py from action_queue import *

namespace QueuedActions {
  // Remy
  public class action_manager {
    /*C#*/private static int len(IList list) => list.Count;
    /*C#*/public action_queue queue = new action_queue();
    /*C#*/public List<action_queue> active_layers = new List<action_queue>();
    //py def __init__(this):
    //py     this.queue = action_queue()
    //py     this.active_layers = []
    public float wait_time(/*this*/) {
      return this.queue.wait_time;
    }
    public void reset_all_actions(/*this*/) {
      this.queue.reset_all_actions();
      this.active_layers.Clear();
    }
    public void reset_current_main_action(/*this*/) {
      this.queue.current.reset();
      for (int i = 0; i < len(this.active_layers); i++) {
        int lastAction = this.active_layers[i].count() - 1;
        if (lastAction >= 0 && this.active_layers[i].get_record(lastAction) == this.queue.current) {
          this.active_layers[i].remove_at(lastAction);
        }
      }
    }
    public bool update(/*this,*/float deltaTime) {
      bool changeHappened = this.queue.update_self_and_distribute(deltaTime, this.active_layers);
      for (int i = 0; i < len(this.active_layers); i++) {
        action_queue layer = this.active_layers[i];
        if (layer == null) {
          continue;
        }
        if (layer.update(deltaTime)) {
          changeHappened = true;
        }
      }
      return changeHappened;
    }
    public void append(/*this,*/action_entry entry) {
      this.queue.append(entry);
    }
    public void insert_next(/*this,*/action_entry entry) {
      this.queue.insert_next(entry);
    }
    public string print_stuff(/*this*/) {
      string output = "";
      output += $"(main) {this.queue}            \n";
      for (int i = 0; i < len(this.active_layers); i++) {
        action_queue layer = this.active_layers[i];
        output += ($"({i}) {layer}               \n");
      }
      output += "                                                       ";
      return output;
    }
  }
}
