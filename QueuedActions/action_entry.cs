using System;
using System.Collections;

namespace QueuedActions {
/*C#*/// style looks like python because this needs to be converted into python
/*C#*/// lines starting with '/*C#*/' will be culled from the translated python file
/*C#*/// lines starting with '//py ' will be inserted into the translated python file
  public class action_entry_state {
    /*C#*/public bool started;
    /*C#*/public int finishConditionBitMask;
    //py def __init__(this):
    //py     this.started = False
    //py     this.finishConditionBitMask = 0
    public void reset(/*this*/) {
      this.started = false;
      this.finishConditionBitMask = 0;
    }
  }
  public class action_entry_conditional {
    /*C#*/public string name;
    /*C#*/public Func<object, bool> check;
    /*C#*/public Action<object> activate;
    /*C#*/public action_entry_conditional(/*this,*/string name, Func<object, bool> check, Action<object> action) {
    //py def __init__(this, name, check=None, action=None):
        this.name = name;
        this.check = check;
        this.activate = action;
    }
      /*C#*/public action_entry_conditional(/*this,*/string name, Func<object, bool> check) {
          /*C#*/this.name = name;
          /*C#*/this.check = check;
      /*C#*/}
  }
  public class action_entry {
    /*C#*/private static int len(IList list) => list.Count;
    /*C#*/public string id;
    /*C#*/public Action<object> what_to_do;
    /*C#*/public Action<object> on_reset;
    /*C#*/public float wait_time;
    /*C#*/public action_entry_conditional[] conditionals;
    /*C#*/public byte blocking_layer_mask;
    /*C#*/public action_entry(/*this,*/string name) {
    //py def __init__(this, name=None):
      this.id = name;
      this.what_to_do = null;
      this.on_reset = null;
      this.wait_time = 0;
      this.conditionals = null;
      this.blocking_layer_mask = 0;
    }
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
        this.what_to_do(record);
      }
      record.state.started = true;
    }
    public int is_finished_check(/*this,*/action_entry_record actionRecord) {
      if (actionRecord.state.finishConditionBitMask != 0) {
        return actionRecord.state.finishConditionBitMask;
      }
      if (this.conditionals == null) {
        actionRecord.state.finishConditionBitMask = -1; 
        return actionRecord.state.finishConditionBitMask;
      }
      int PassByDefault = -1;
      int finishedMaskByDefault = PassByDefault;
      for (int i = 0; i < len(this.conditionals); i++) {
        action_entry_conditional condition = this.conditionals[i];
        if (condition == null) {
          continue;
        }
        if (condition.check == null && condition.activate == null) {
          finishedMaskByDefault = 1 << i;
          continue;
        }
        if (condition.check == null || condition.check(actionRecord)) {
          if (condition.activate != null) {
            condition.activate(actionRecord);
          }
          actionRecord.state.finishConditionBitMask = 1 << i;
          return actionRecord.state.finishConditionBitMask;
        }
        if (finishedMaskByDefault == PassByDefault) {
          finishedMaskByDefault = 0;
        }
      }
      actionRecord.state.finishConditionBitMask = finishedMaskByDefault;
      return finishedMaskByDefault;
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
      return this.state.finishConditionBitMask != 0 || this.entry.is_finished_check(this) != 0;
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
