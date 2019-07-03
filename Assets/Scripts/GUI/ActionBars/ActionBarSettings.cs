using UnityEngine;
using System.Collections.Generic;

public class ActionBarSettings : MonoBehaviour 
{
	public List<UIAtlas> Atlas;	
	public AudioClip ButtonClickSound_Success;
	public AudioClip ButtonClickSound_Disable;
	public AudioClip ButtonSound_Destroyed;
	public AudioClip ButtonSound_Swap;
	public bool ShowKeyBindings = true;
	public bool DisplayCooldownTimer = true; 
	static ActionBarSettings mInstance;
	public Dictionary<KeyCode, string> HotKeyDictionary = new Dictionary<KeyCode, string>();
	private int InstanceNumberTracker = 0;
    public static ActionBarSettings Instance
    {
		
        get { return mInstance ?? (mInstance = FindObjectOfType(typeof (ActionBarSettings)) as ActionBarSettings); }
    }
	//Returns a unique number for each item/spell
	public int GetInstanceNumber()
	{
		return InstanceNumberTracker++;
	}
	
	
	ActionBarSettings()
	{	
		HotKeyDictionary.Add(KeyCode.None,"");
		HotKeyDictionary.Add(KeyCode.A,"A");
		HotKeyDictionary.Add(KeyCode.Alpha0,"0");
		HotKeyDictionary.Add(KeyCode.Alpha1,"1");
		HotKeyDictionary.Add(KeyCode.Alpha2,"2");
		HotKeyDictionary.Add(KeyCode.Alpha3,"3");
		HotKeyDictionary.Add(KeyCode.Alpha4,"4");
		HotKeyDictionary.Add(KeyCode.Alpha5,"5");
		HotKeyDictionary.Add(KeyCode.Alpha6,"6");
		HotKeyDictionary.Add(KeyCode.Alpha7,"7");
		HotKeyDictionary.Add(KeyCode.Alpha8,"8");
		HotKeyDictionary.Add(KeyCode.Alpha9,"9");
		HotKeyDictionary.Add(KeyCode.Ampersand,"&");
		HotKeyDictionary.Add(KeyCode.Asterisk,"*");
		HotKeyDictionary.Add(KeyCode.At,"@");
		HotKeyDictionary.Add(KeyCode.B,"B");
		HotKeyDictionary.Add(KeyCode.BackQuote,"`");
		HotKeyDictionary.Add(KeyCode.Backslash, @"\");
		HotKeyDictionary.Add(KeyCode.Backspace,"BS");
		HotKeyDictionary.Add(KeyCode.Break,"BRK");
		HotKeyDictionary.Add(KeyCode.C,"C");
		HotKeyDictionary.Add(KeyCode.CapsLock,"CAP");
		HotKeyDictionary.Add(KeyCode.Caret,"^");
		HotKeyDictionary.Add(KeyCode.Clear,"CLR");
		HotKeyDictionary.Add(KeyCode.Colon,":");
		HotKeyDictionary.Add(KeyCode.Comma,",");
		HotKeyDictionary.Add(KeyCode.D,"D");
		HotKeyDictionary.Add(KeyCode.Delete,"DEL");
		HotKeyDictionary.Add(KeyCode.Dollar,"$");
		HotKeyDictionary.Add(KeyCode.DownArrow,"DRW");
		HotKeyDictionary.Add(KeyCode.E,"E");
		HotKeyDictionary.Add(KeyCode.End,"END");
		HotKeyDictionary.Add(KeyCode.Equals,"=");
		HotKeyDictionary.Add(KeyCode.Escape,"ESC");
		HotKeyDictionary.Add(KeyCode.Exclaim,"!");
		HotKeyDictionary.Add(KeyCode.F,"F");
		HotKeyDictionary.Add(KeyCode.F1,"F1");
		HotKeyDictionary.Add(KeyCode.F10,"F10");
		HotKeyDictionary.Add(KeyCode.F11,"F11");
		HotKeyDictionary.Add(KeyCode.F12,"F12");
		HotKeyDictionary.Add(KeyCode.F13,"F13");
		HotKeyDictionary.Add(KeyCode.F14,"F14");
		HotKeyDictionary.Add(KeyCode.F15,"F15");
		HotKeyDictionary.Add(KeyCode.F2,"F2");
		HotKeyDictionary.Add(KeyCode.F3,"F3");
		HotKeyDictionary.Add(KeyCode.F4,"F4");
		HotKeyDictionary.Add(KeyCode.F5,"F5");
		HotKeyDictionary.Add(KeyCode.F6,"F6");
		HotKeyDictionary.Add(KeyCode.F7,"F7");
		HotKeyDictionary.Add(KeyCode.F8,"F8");
		HotKeyDictionary.Add(KeyCode.F9,"F9");
		HotKeyDictionary.Add(KeyCode.G,"G");
		HotKeyDictionary.Add(KeyCode.Greater,">");
		HotKeyDictionary.Add(KeyCode.H,"H");
		HotKeyDictionary.Add(KeyCode.Hash,"#");
		HotKeyDictionary.Add(KeyCode.Help,"HLP");
		HotKeyDictionary.Add(KeyCode.Home,"HME");
		HotKeyDictionary.Add(KeyCode.I,"I");
		HotKeyDictionary.Add(KeyCode.Insert,"INS");
		HotKeyDictionary.Add(KeyCode.J,"J");
		HotKeyDictionary.Add(KeyCode.K,"K");
		HotKeyDictionary.Add(KeyCode.Keypad0,"KP0");
		HotKeyDictionary.Add(KeyCode.Keypad1,"KP1");
		HotKeyDictionary.Add(KeyCode.Keypad2,"KP2");
		HotKeyDictionary.Add(KeyCode.Keypad3,"KP3");
		HotKeyDictionary.Add(KeyCode.Keypad4,"KP4");
		HotKeyDictionary.Add(KeyCode.Keypad5,"KP5");
		HotKeyDictionary.Add(KeyCode.Keypad6,"KP6");
		HotKeyDictionary.Add(KeyCode.Keypad7,"KP7");
		HotKeyDictionary.Add(KeyCode.Keypad8,"KP8");
		HotKeyDictionary.Add(KeyCode.Keypad9,"KP9");
		HotKeyDictionary.Add(KeyCode.KeypadDivide,"KPD");
		HotKeyDictionary.Add(KeyCode.KeypadEnter,"KEN");
		HotKeyDictionary.Add(KeyCode.KeypadEquals,"K=");
		HotKeyDictionary.Add(KeyCode.KeypadMinus,"K-");
		HotKeyDictionary.Add(KeyCode.KeypadMultiply,"K*");
		HotKeyDictionary.Add(KeyCode.KeypadPeriod,"K.");
		HotKeyDictionary.Add(KeyCode.KeypadPlus,"K+");
		HotKeyDictionary.Add(KeyCode.L,"L");
		HotKeyDictionary.Add(KeyCode.LeftAlt,"ALT");
		HotKeyDictionary.Add(KeyCode.LeftApple,"WIN");
		HotKeyDictionary.Add(KeyCode.LeftArrow,"L<-");
		HotKeyDictionary.Add(KeyCode.LeftBracket,"[");
		HotKeyDictionary.Add(KeyCode.LeftControl,"LCtrl");
		HotKeyDictionary.Add(KeyCode.LeftParen,"(");
		HotKeyDictionary.Add(KeyCode.LeftShift,"LShft");
		HotKeyDictionary.Add(KeyCode.Less,"<");
		HotKeyDictionary.Add(KeyCode.M,"M");
		HotKeyDictionary.Add(KeyCode.Minus,"-");
		HotKeyDictionary.Add(KeyCode.N,"N");
		HotKeyDictionary.Add(KeyCode.Numlock,"Num");
		HotKeyDictionary.Add(KeyCode.O,"O");
		HotKeyDictionary.Add(KeyCode.P,"P");
		HotKeyDictionary.Add(KeyCode.PageDown,"PGD");
		HotKeyDictionary.Add(KeyCode.PageUp,"PGU");
		HotKeyDictionary.Add(KeyCode.Pause,"PAUS");
		HotKeyDictionary.Add(KeyCode.Period,".");
		HotKeyDictionary.Add(KeyCode.Plus,"+");
		HotKeyDictionary.Add(KeyCode.Print,"PNT");
		HotKeyDictionary.Add(KeyCode.Q,"Q");
		HotKeyDictionary.Add(KeyCode.Question,"?");
		HotKeyDictionary.Add(KeyCode.Quote,'"'.ToString());
		HotKeyDictionary.Add(KeyCode.R,"R");
		HotKeyDictionary.Add(KeyCode.Return,"RET");
		HotKeyDictionary.Add(KeyCode.RightAlt,"RALT");
		HotKeyDictionary.Add(KeyCode.RightArrow,"->");
		HotKeyDictionary.Add(KeyCode.RightBracket,"]");
		HotKeyDictionary.Add(KeyCode.RightControl,"RCtrl");
		HotKeyDictionary.Add(KeyCode.RightParen,")");
		HotKeyDictionary.Add(KeyCode.RightShift,"RSFT");
		HotKeyDictionary.Add(KeyCode.S,"S");
		HotKeyDictionary.Add(KeyCode.Semicolon,";");
		HotKeyDictionary.Add(KeyCode.Slash,"/");
		HotKeyDictionary.Add(KeyCode.Space, "SPC");
		HotKeyDictionary.Add(KeyCode.T,"T");
		HotKeyDictionary.Add(KeyCode.Tab,"TAB");
		HotKeyDictionary.Add(KeyCode.U,"U");
		HotKeyDictionary.Add(KeyCode.Underscore,"_");
		HotKeyDictionary.Add(KeyCode.UpArrow,"UPA");
		HotKeyDictionary.Add(KeyCode.V,"V");
		HotKeyDictionary.Add(KeyCode.W,"W");
		HotKeyDictionary.Add(KeyCode.X,"X");
		HotKeyDictionary.Add(KeyCode.Y,"Y");
		HotKeyDictionary.Add(KeyCode.Z,"Z");
	}
}
