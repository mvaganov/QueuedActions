using System;
using System.Collections.Generic;

namespace QueuedActions {
  public class action_manager {
    /// <summary>
    /// processes to queue up
    /// </summary>
    public action_queue queue = new action_queue();
    /// <summary>
    /// parallel tasks to accomplish
    /// </summary>
    public List<action_queue> active_layers = new List<action_queue>();
    public float wait_time(/*this*/) {
      return this.queue.wait_time;
    }
    public void reset_all_actions(/*this*/) {
      this.queue.reset_all_actions();
      this.active_layers.Clear();
    }
    public void reset_current_main_action(/*this*/) {
      this.queue.current.reset();
      for (int i = 0; i < this.active_layers.Count; i++) {
        int lastAction = this.active_layers[i].count() - 1;
        if (lastAction >= 0 && this.active_layers[i].get_record(lastAction) == this.queue.current) {
          this.active_layers[i].remove_at(lastAction);
        }
      }
    }
    public void update(/*this,*/float deltaTime) {
      this.queue.update_self_and_distribute(deltaTime, this.active_layers);
      for (int i = 0; i < this.active_layers.Count; i++) {
        action_queue layer = this.active_layers[i];
        if (layer == null) {
          continue;
        }
        layer.update(deltaTime);
      }
    }
    public void append(/*this,*/action_entry entry) {
      this.queue.append(entry);
    }
    public void insert_next(/*this,*/action_entry entry) {
      this.queue.insert_next(entry);
    }
    public void print_stuff(/*this*/) {
      Console.WriteLine($"(main) {this.queue}                                    ");
      for (int layer = 0; layer < this.active_layers.Count; ++layer) {
        action_queue processLayer = this.active_layers[layer];
        Console.WriteLine($"({layer}) {processLayer}                                       ");
      }
    }
  }
}
