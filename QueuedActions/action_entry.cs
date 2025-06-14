using System;
using System.Collections;

namespace QueuedActions {
/*C#*/// style looks like python because this needs to be converted into python
  public class action_entry_state {
    /*C#*/public bool started;
    /*C#*/public bool finished;
    //py def __init__(this):
    //py     this.started = False
    //py     this.finished = False
    public void reset(/*this*/) {
      this.started = false;
      this.finished = false;
    }
  }
  public class action_entry_conditional {
    /*C#*/public string name;
    /*C#*/public Func<action_entry_record, bool> check;
    /*C#*/public Action<action_entry_record> activate;
    /*C#*/public action_entry_conditional(/*this,*/string name, Func<action_entry_record, bool> check, Action<action_entry_record> action) {
    //py def __init__(this, name, check=None, action=None):
        this.name = name;
        this.check = check;
        this.activate = action;
    }
      /*C#*/public action_entry_conditional(/*this,*/string name, Func<action_entry_record, bool> check) {
          /*C#*/this.name = name;
          /*C#*/this.check = check;
      /*C#*/}
  }
  public class action_entry {
    /*C#*/private static int len(IList list) => list.Count;
    /*C#*/public string id;
    /*C#*/public Action what_to_do;
    /*C#*/public Action<action_entry_record> on_reset;
    /*C#*/public float wait_time;
    /*C#*/public action_entry_conditional[] conditionals;
    /*C#*/public byte blocking_layer_mask;
    //py def __init__(this):
    //py     this.id = ""
    //py     this.what_to_do = None
    //py     this.on_reset = None
    //py     this.wait_time = 0
    //py     this.conditionals = [""]
    //py     this.blocking_layer_mask = 0

    public bool uses_layers(/*this*/) {
      return this.blocking_layer_mask != 0;
    }
    public bool is_layer(/*this,*/int layer) {
      return (this.blocking_layer_mask & (1 << layer)) != 0;
    }
    public bool set_layer(/*this,*/int layer, bool blocked) {
      if (layer >= 255 || layer < 0) {
        return false;
      }
      if (blocked) {
        /*C#*/this.blocking_layer_mask &= (byte)layer;
        //py this.blocking_layer_mask &= layer;
      } else {
        /*C#*/this.blocking_layer_mask &= (byte)~layer;
        //py this.blocking_layer_mask &= ~layer;
      }
      return true;
    }
    public void start(/*this,*/action_entry_record record) {
      if (this.what_to_do != null) {
        this.what_to_do();
      }
      record.state.started = true;
    }
    public bool is_finished_check(/*this,*/action_entry_record actionRecord) {
      if (actionRecord.state.finished) {
        return true;
      }
      if (this.conditionals == null) {
        actionRecord.state.finished = true; 
        return true;
      }
      bool finishByDefault = true;
      for (int i = 0; i < len(this.conditionals); i++) {
        action_entry_conditional condition = this.conditionals[i];
        if (condition == null || (condition.check == null && condition.activate == null)) {
          continue;
        }
        finishByDefault = false;
        if (condition.check == null || condition.check(actionRecord)) {
          if (condition.activate != null) {
            condition.activate(actionRecord);
          }
          actionRecord.state.finished = true;
          return true;
        }
      }
      actionRecord.state.finished = finishByDefault;
      return finishByDefault;
    }
    public action_entry_conditional get_state(/*this,*/string name) {
      if (this.conditionals == null) { return null; }
      for (int i = 0; i < len(this.conditionals); i++) {
        if (this.conditionals[i].name == name) {
          return this.conditionals[i];
        }
      }
      return null;
    }
  }
  public class action_entry_record {
    /*C#*/public action_entry_state state;
    /*C#*/public action_entry entry;
    /*C#*/public action_entry_record(/*this,*/action_entry entry) {
    //py def __init__(this, entry):
      this.state = new action_entry_state();
      this.entry = entry;
    }
    public void reset(/*this*/) {
      if (this.entry.on_reset != null) {
        this.entry.on_reset(this);
      }
      this.state.reset();
    }
    public bool is_started(/*this*/) {
      return this.state.started;
    }
    public bool is_finished(/*this*/) {
      return this.state.finished || this.entry.is_finished_check(this);
    }
    public void start(/*this*/) {
      this.entry.start(this);
    }
    public float wait_time(/*this*/) {
      return this.entry.wait_time;
    }
    public bool uses_layers(/*this*/) {
      return this.entry.uses_layers();
    }
  }
}
