using System;
using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;


namespace NOAH.UI
{
    public sealed partial class UIWindowConfig : pb::IMessage<UIWindowConfig>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        , pb::IBufferMessage
#endif
    {
        private static readonly pb::MessageParser<UIWindowConfig> _parser = new pb::MessageParser<UIWindowConfig>(() => new UIWindowConfig());
        private pb::UnknownFieldSet _unknownFields;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pb::MessageParser<UIWindowConfig> Parser
        {
            get { return _parser; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pbr::MessageDescriptor Descriptor
        {
            // get { return global::XlsxReflection.Descriptor.MessageTypes[3]; }
            get { throw new NotImplementedException(); }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public UIWindowConfig()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public UIWindowConfig(UIWindowConfig other) : this()
        {
            name_ = other.name_;
            dontDestory_ = other.dontDestory_;
            sortingOrder_ = other.sortingOrder_;
            fullScreen_ = other.fullScreen_;
            allowActorControl_ = other.allowActorControl_;
            instructionOffsetX_ = other.instructionOffsetX_;
            instructionOffsetY_ = other.instructionOffsetY_;
            penetrate_ = other.penetrate_;
            statusBarStyle_ = other.statusBarStyle_;
            statusBarRightPadding_ = other.statusBarRightPadding_;
            bgmCue_ = other.bgmCue_;
            bgmBlock_ = other.bgmBlock_;
            revertPreviousBgm_ = other.revertPreviousBgm_;
            openSE_ = other.openSE_;
            closeSE_ = other.closeSE_;
            bgmCut_ = other.bgmCut_;
            showRollingMessage_ = other.showRollingMessage_;
            keepCurrentMessage_ = other.keepCurrentMessage_;
            currentMessagePosX_ = other.currentMessagePosX_;
            currentMessagePosY_ = other.currentMessagePosY_;
            title_ = other.title_;
            _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public UIWindowConfig Clone()
        {
            return new UIWindowConfig(this);
        }

        /// <summary>Field number for the "name" field.</summary>
        public const int NameFieldNumber = 1;

        private string name_ = "";

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string Name
        {
            get { return name_; }
            set { name_ = pb::ProtoPreconditions.CheckNotNull(value, "value"); }
        }

        /// <summary>Field number for the "dontDestory" field.</summary>
        public const int DontDestoryFieldNumber = 2;

        private bool dontDestory_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool DontDestory
        {
            get { return dontDestory_; }
            set { dontDestory_ = value; }
        }

        /// <summary>Field number for the "sortingOrder" field.</summary>
        public const int SortingOrderFieldNumber = 3;

        private int sortingOrder_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int SortingOrder
        {
            get { return sortingOrder_; }
            set { sortingOrder_ = value; }
        }

        /// <summary>Field number for the "fullScreen" field.</summary>
        public const int FullScreenFieldNumber = 4;

        private bool fullScreen_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool FullScreen
        {
            get { return fullScreen_; }
            set { fullScreen_ = value; }
        }

        /// <summary>Field number for the "allowActorControl" field.</summary>
        public const int AllowActorControlFieldNumber = 5;

        private bool allowActorControl_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool AllowActorControl
        {
            get { return allowActorControl_; }
            set { allowActorControl_ = value; }
        }

        /// <summary>Field number for the "instructionOffsetX" field.</summary>
        public const int InstructionOffsetXFieldNumber = 6;

        private int instructionOffsetX_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int InstructionOffsetX
        {
            get { return instructionOffsetX_; }
            set { instructionOffsetX_ = value; }
        }

        /// <summary>Field number for the "instructionOffsetY" field.</summary>
        public const int InstructionOffsetYFieldNumber = 7;

        private int instructionOffsetY_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int InstructionOffsetY
        {
            get { return instructionOffsetY_; }
            set { instructionOffsetY_ = value; }
        }

        /// <summary>Field number for the "penetrate" field.</summary>
        public const int PenetrateFieldNumber = 14;

        private bool penetrate_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool Penetrate
        {
            get { return penetrate_; }
            set { penetrate_ = value; }
        }

        /// <summary>Field number for the "statusBarStyle" field.</summary>
        public const int StatusBarStyleFieldNumber = 8;

        private int statusBarStyle_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int StatusBarStyle
        {
            get { return statusBarStyle_; }
            set { statusBarStyle_ = value; }
        }

        /// <summary>Field number for the "statusBarRightPadding" field.</summary>
        public const int StatusBarRightPaddingFieldNumber = 9;

        private int statusBarRightPadding_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int StatusBarRightPadding
        {
            get { return statusBarRightPadding_; }
            set { statusBarRightPadding_ = value; }
        }

        /// <summary>Field number for the "bgmCue" field.</summary>
        public const int BgmCueFieldNumber = 11;

        private string bgmCue_ = "";

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string BgmCue
        {
            get { return bgmCue_; }
            set { bgmCue_ = pb::ProtoPreconditions.CheckNotNull(value, "value"); }
        }

        /// <summary>Field number for the "bgmBlock" field.</summary>
        public const int BgmBlockFieldNumber = 12;

        private string bgmBlock_ = "";

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string BgmBlock
        {
            get { return bgmBlock_; }
            set { bgmBlock_ = pb::ProtoPreconditions.CheckNotNull(value, "value"); }
        }

        /// <summary>Field number for the "revertPreviousBgm" field.</summary>
        public const int RevertPreviousBgmFieldNumber = 13;

        private bool revertPreviousBgm_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool RevertPreviousBgm
        {
            get { return revertPreviousBgm_; }
            set { revertPreviousBgm_ = value; }
        }

        /// <summary>Field number for the "openSE" field.</summary>
        public const int OpenSEFieldNumber = 15;

        private string openSE_ = "";

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string OpenSE
        {
            get { return openSE_; }
            set { openSE_ = pb::ProtoPreconditions.CheckNotNull(value, "value"); }
        }

        /// <summary>Field number for the "closeSE" field.</summary>
        public const int CloseSEFieldNumber = 16;

        private string closeSE_ = "";

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string CloseSE
        {
            get { return closeSE_; }
            set { closeSE_ = pb::ProtoPreconditions.CheckNotNull(value, "value"); }
        }

        /// <summary>Field number for the "bgmCut" field.</summary>
        public const int BgmCutFieldNumber = 17;

        private int bgmCut_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int BgmCut
        {
            get { return bgmCut_; }
            set { bgmCut_ = value; }
        }

        /// <summary>Field number for the "showRollingMessage" field.</summary>
        public const int ShowRollingMessageFieldNumber = 20;

        private bool showRollingMessage_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool ShowRollingMessage
        {
            get { return showRollingMessage_; }
            set { showRollingMessage_ = value; }
        }

        /// <summary>Field number for the "keepCurrentMessage" field.</summary>
        public const int KeepCurrentMessageFieldNumber = 21;

        private bool keepCurrentMessage_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool KeepCurrentMessage
        {
            get { return keepCurrentMessage_; }
            set { keepCurrentMessage_ = value; }
        }

        /// <summary>Field number for the "currentMessagePosX" field.</summary>
        public const int CurrentMessagePosXFieldNumber = 22;

        private int currentMessagePosX_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int CurrentMessagePosX
        {
            get { return currentMessagePosX_; }
            set { currentMessagePosX_ = value; }
        }

        /// <summary>Field number for the "currentMessagePosY" field.</summary>
        public const int CurrentMessagePosYFieldNumber = 23;

        private int currentMessagePosY_;

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int CurrentMessagePosY
        {
            get { return currentMessagePosY_; }
            set { currentMessagePosY_ = value; }
        }

        /// <summary>Field number for the "title" field.</summary>
        public const int TitleFieldNumber = 24;

        private string title_ = "";

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string Title
        {
            get { return title_; }
            set { title_ = pb::ProtoPreconditions.CheckNotNull(value, "value"); }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override bool Equals(object other)
        {
            return Equals(other as UIWindowConfig);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool Equals(UIWindowConfig other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            if (Name != other.Name) return false;
            if (DontDestory != other.DontDestory) return false;
            if (SortingOrder != other.SortingOrder) return false;
            if (FullScreen != other.FullScreen) return false;
            if (AllowActorControl != other.AllowActorControl) return false;
            if (InstructionOffsetX != other.InstructionOffsetX) return false;
            if (InstructionOffsetY != other.InstructionOffsetY) return false;
            if (Penetrate != other.Penetrate) return false;
            if (StatusBarStyle != other.StatusBarStyle) return false;
            if (StatusBarRightPadding != other.StatusBarRightPadding) return false;
            if (BgmCue != other.BgmCue) return false;
            if (BgmBlock != other.BgmBlock) return false;
            if (RevertPreviousBgm != other.RevertPreviousBgm) return false;
            if (OpenSE != other.OpenSE) return false;
            if (CloseSE != other.CloseSE) return false;
            if (BgmCut != other.BgmCut) return false;
            if (ShowRollingMessage != other.ShowRollingMessage) return false;
            if (KeepCurrentMessage != other.KeepCurrentMessage) return false;
            if (CurrentMessagePosX != other.CurrentMessagePosX) return false;
            if (CurrentMessagePosY != other.CurrentMessagePosY) return false;
            if (Title != other.Title) return false;
            return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override int GetHashCode()
        {
            int hash = 1;
            if (Name.Length != 0) hash ^= Name.GetHashCode();
            if (DontDestory != false) hash ^= DontDestory.GetHashCode();
            if (SortingOrder != 0) hash ^= SortingOrder.GetHashCode();
            if (FullScreen != false) hash ^= FullScreen.GetHashCode();
            if (AllowActorControl != false) hash ^= AllowActorControl.GetHashCode();
            if (InstructionOffsetX != 0) hash ^= InstructionOffsetX.GetHashCode();
            if (InstructionOffsetY != 0) hash ^= InstructionOffsetY.GetHashCode();
            if (Penetrate != false) hash ^= Penetrate.GetHashCode();
            if (StatusBarStyle != 0) hash ^= StatusBarStyle.GetHashCode();
            if (StatusBarRightPadding != 0) hash ^= StatusBarRightPadding.GetHashCode();
            if (BgmCue.Length != 0) hash ^= BgmCue.GetHashCode();
            if (BgmBlock.Length != 0) hash ^= BgmBlock.GetHashCode();
            if (RevertPreviousBgm != false) hash ^= RevertPreviousBgm.GetHashCode();
            if (OpenSE.Length != 0) hash ^= OpenSE.GetHashCode();
            if (CloseSE.Length != 0) hash ^= CloseSE.GetHashCode();
            if (BgmCut != 0) hash ^= BgmCut.GetHashCode();
            if (ShowRollingMessage != false) hash ^= ShowRollingMessage.GetHashCode();
            if (KeepCurrentMessage != false) hash ^= KeepCurrentMessage.GetHashCode();
            if (CurrentMessagePosX != 0) hash ^= CurrentMessagePosX.GetHashCode();
            if (CurrentMessagePosY != 0) hash ^= CurrentMessagePosY.GetHashCode();
            if (Title.Length != 0) hash ^= Title.GetHashCode();
            if (_unknownFields != null)
            {
                hash ^= _unknownFields.GetHashCode();
            }

            return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override string ToString()
        {
            return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void WriteTo(pb::CodedOutputStream output)
        {
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
            output.WriteRawMessage(this);
#else
    if (Name.Length != 0) {
      output.WriteRawTag(10);
      output.WriteString(Name);
    }
    if (DontDestory != false) {
      output.WriteRawTag(16);
      output.WriteBool(DontDestory);
    }
    if (SortingOrder != 0) {
      output.WriteRawTag(24);
      output.WriteInt32(SortingOrder);
    }
    if (FullScreen != false) {
      output.WriteRawTag(32);
      output.WriteBool(FullScreen);
    }
    if (AllowActorControl != false) {
      output.WriteRawTag(40);
      output.WriteBool(AllowActorControl);
    }
    if (InstructionOffsetX != 0) {
      output.WriteRawTag(48);
      output.WriteInt32(InstructionOffsetX);
    }
    if (InstructionOffsetY != 0) {
      output.WriteRawTag(56);
      output.WriteInt32(InstructionOffsetY);
    }
    if (StatusBarStyle != 0) {
      output.WriteRawTag(64);
      output.WriteInt32(StatusBarStyle);
    }
    if (StatusBarRightPadding != 0) {
      output.WriteRawTag(72);
      output.WriteInt32(StatusBarRightPadding);
    }
    if (BgmCue.Length != 0) {
      output.WriteRawTag(90);
      output.WriteString(BgmCue);
    }
    if (BgmBlock.Length != 0) {
      output.WriteRawTag(98);
      output.WriteString(BgmBlock);
    }
    if (RevertPreviousBgm != false) {
      output.WriteRawTag(104);
      output.WriteBool(RevertPreviousBgm);
    }
    if (Penetrate != false) {
      output.WriteRawTag(112);
      output.WriteBool(Penetrate);
    }
    if (OpenSE.Length != 0) {
      output.WriteRawTag(122);
      output.WriteString(OpenSE);
    }
    if (CloseSE.Length != 0) {
      output.WriteRawTag(130, 1);
      output.WriteString(CloseSE);
    }
    if (BgmCut != 0) {
      output.WriteRawTag(136, 1);
      output.WriteInt32(BgmCut);
    }
    if (ShowRollingMessage != false) {
      output.WriteRawTag(160, 1);
      output.WriteBool(ShowRollingMessage);
    }
    if (KeepCurrentMessage != false) {
      output.WriteRawTag(168, 1);
      output.WriteBool(KeepCurrentMessage);
    }
    if (CurrentMessagePosX != 0) {
      output.WriteRawTag(176, 1);
      output.WriteInt32(CurrentMessagePosX);
    }
    if (CurrentMessagePosY != 0) {
      output.WriteRawTag(184, 1);
      output.WriteInt32(CurrentMessagePosY);
    }
    if (Title.Length != 0) {
      output.WriteRawTag(194, 1);
      output.WriteString(Title);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
#endif
        }

#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output)
        {
            if (Name.Length != 0)
            {
                output.WriteRawTag(10);
                output.WriteString(Name);
            }

            if (DontDestory != false)
            {
                output.WriteRawTag(16);
                output.WriteBool(DontDestory);
            }

            if (SortingOrder != 0)
            {
                output.WriteRawTag(24);
                output.WriteInt32(SortingOrder);
            }

            if (FullScreen != false)
            {
                output.WriteRawTag(32);
                output.WriteBool(FullScreen);
            }

            if (AllowActorControl != false)
            {
                output.WriteRawTag(40);
                output.WriteBool(AllowActorControl);
            }

            if (InstructionOffsetX != 0)
            {
                output.WriteRawTag(48);
                output.WriteInt32(InstructionOffsetX);
            }

            if (InstructionOffsetY != 0)
            {
                output.WriteRawTag(56);
                output.WriteInt32(InstructionOffsetY);
            }

            if (StatusBarStyle != 0)
            {
                output.WriteRawTag(64);
                output.WriteInt32(StatusBarStyle);
            }

            if (StatusBarRightPadding != 0)
            {
                output.WriteRawTag(72);
                output.WriteInt32(StatusBarRightPadding);
            }

            if (BgmCue.Length != 0)
            {
                output.WriteRawTag(90);
                output.WriteString(BgmCue);
            }

            if (BgmBlock.Length != 0)
            {
                output.WriteRawTag(98);
                output.WriteString(BgmBlock);
            }

            if (RevertPreviousBgm != false)
            {
                output.WriteRawTag(104);
                output.WriteBool(RevertPreviousBgm);
            }

            if (Penetrate != false)
            {
                output.WriteRawTag(112);
                output.WriteBool(Penetrate);
            }

            if (OpenSE.Length != 0)
            {
                output.WriteRawTag(122);
                output.WriteString(OpenSE);
            }

            if (CloseSE.Length != 0)
            {
                output.WriteRawTag(130, 1);
                output.WriteString(CloseSE);
            }

            if (BgmCut != 0)
            {
                output.WriteRawTag(136, 1);
                output.WriteInt32(BgmCut);
            }

            if (ShowRollingMessage != false)
            {
                output.WriteRawTag(160, 1);
                output.WriteBool(ShowRollingMessage);
            }

            if (KeepCurrentMessage != false)
            {
                output.WriteRawTag(168, 1);
                output.WriteBool(KeepCurrentMessage);
            }

            if (CurrentMessagePosX != 0)
            {
                output.WriteRawTag(176, 1);
                output.WriteInt32(CurrentMessagePosX);
            }

            if (CurrentMessagePosY != 0)
            {
                output.WriteRawTag(184, 1);
                output.WriteInt32(CurrentMessagePosY);
            }

            if (Title.Length != 0)
            {
                output.WriteRawTag(194, 1);
                output.WriteString(Title);
            }

            if (_unknownFields != null)
            {
                _unknownFields.WriteTo(ref output);
            }
        }
#endif

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int CalculateSize()
        {
            int size = 0;
            if (Name.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
            }

            if (DontDestory != false)
            {
                size += 1 + 1;
            }

            if (SortingOrder != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt32Size(SortingOrder);
            }

            if (FullScreen != false)
            {
                size += 1 + 1;
            }

            if (AllowActorControl != false)
            {
                size += 1 + 1;
            }

            if (InstructionOffsetX != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt32Size(InstructionOffsetX);
            }

            if (InstructionOffsetY != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt32Size(InstructionOffsetY);
            }

            if (Penetrate != false)
            {
                size += 1 + 1;
            }

            if (StatusBarStyle != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt32Size(StatusBarStyle);
            }

            if (StatusBarRightPadding != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt32Size(StatusBarRightPadding);
            }

            if (BgmCue.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(BgmCue);
            }

            if (BgmBlock.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(BgmBlock);
            }

            if (RevertPreviousBgm != false)
            {
                size += 1 + 1;
            }

            if (OpenSE.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(OpenSE);
            }

            if (CloseSE.Length != 0)
            {
                size += 2 + pb::CodedOutputStream.ComputeStringSize(CloseSE);
            }

            if (BgmCut != 0)
            {
                size += 2 + pb::CodedOutputStream.ComputeInt32Size(BgmCut);
            }

            if (ShowRollingMessage != false)
            {
                size += 2 + 1;
            }

            if (KeepCurrentMessage != false)
            {
                size += 2 + 1;
            }

            if (CurrentMessagePosX != 0)
            {
                size += 2 + pb::CodedOutputStream.ComputeInt32Size(CurrentMessagePosX);
            }

            if (CurrentMessagePosY != 0)
            {
                size += 2 + pb::CodedOutputStream.ComputeInt32Size(CurrentMessagePosY);
            }

            if (Title.Length != 0)
            {
                size += 2 + pb::CodedOutputStream.ComputeStringSize(Title);
            }

            if (_unknownFields != null)
            {
                size += _unknownFields.CalculateSize();
            }

            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(UIWindowConfig other)
        {
            if (other == null)
            {
                return;
            }

            if (other.Name.Length != 0)
            {
                Name = other.Name;
            }

            if (other.DontDestory != false)
            {
                DontDestory = other.DontDestory;
            }

            if (other.SortingOrder != 0)
            {
                SortingOrder = other.SortingOrder;
            }

            if (other.FullScreen != false)
            {
                FullScreen = other.FullScreen;
            }

            if (other.AllowActorControl != false)
            {
                AllowActorControl = other.AllowActorControl;
            }

            if (other.InstructionOffsetX != 0)
            {
                InstructionOffsetX = other.InstructionOffsetX;
            }

            if (other.InstructionOffsetY != 0)
            {
                InstructionOffsetY = other.InstructionOffsetY;
            }

            if (other.Penetrate != false)
            {
                Penetrate = other.Penetrate;
            }

            if (other.StatusBarStyle != 0)
            {
                StatusBarStyle = other.StatusBarStyle;
            }

            if (other.StatusBarRightPadding != 0)
            {
                StatusBarRightPadding = other.StatusBarRightPadding;
            }

            if (other.BgmCue.Length != 0)
            {
                BgmCue = other.BgmCue;
            }

            if (other.BgmBlock.Length != 0)
            {
                BgmBlock = other.BgmBlock;
            }

            if (other.RevertPreviousBgm != false)
            {
                RevertPreviousBgm = other.RevertPreviousBgm;
            }

            if (other.OpenSE.Length != 0)
            {
                OpenSE = other.OpenSE;
            }

            if (other.CloseSE.Length != 0)
            {
                CloseSE = other.CloseSE;
            }

            if (other.BgmCut != 0)
            {
                BgmCut = other.BgmCut;
            }

            if (other.ShowRollingMessage != false)
            {
                ShowRollingMessage = other.ShowRollingMessage;
            }

            if (other.KeepCurrentMessage != false)
            {
                KeepCurrentMessage = other.KeepCurrentMessage;
            }

            if (other.CurrentMessagePosX != 0)
            {
                CurrentMessagePosX = other.CurrentMessagePosX;
            }

            if (other.CurrentMessagePosY != 0)
            {
                CurrentMessagePosY = other.CurrentMessagePosY;
            }

            if (other.Title.Length != 0)
            {
                Title = other.Title;
            }

            _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(pb::CodedInputStream input)
        {
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
            input.ReadRawMessage(this);
#else
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          Name = input.ReadString();
          break;
        }
        case 16: {
          DontDestory = input.ReadBool();
          break;
        }
        case 24: {
          SortingOrder = input.ReadInt32();
          break;
        }
        case 32: {
          FullScreen = input.ReadBool();
          break;
        }
        case 40: {
          AllowActorControl = input.ReadBool();
          break;
        }
        case 48: {
          InstructionOffsetX = input.ReadInt32();
          break;
        }
        case 56: {
          InstructionOffsetY = input.ReadInt32();
          break;
        }
        case 64: {
          StatusBarStyle = input.ReadInt32();
          break;
        }
        case 72: {
          StatusBarRightPadding = input.ReadInt32();
          break;
        }
        case 90: {
          BgmCue = input.ReadString();
          break;
        }
        case 98: {
          BgmBlock = input.ReadString();
          break;
        }
        case 104: {
          RevertPreviousBgm = input.ReadBool();
          break;
        }
        case 112: {
          Penetrate = input.ReadBool();
          break;
        }
        case 122: {
          OpenSE = input.ReadString();
          break;
        }
        case 130: {
          CloseSE = input.ReadString();
          break;
        }
        case 136: {
          BgmCut = input.ReadInt32();
          break;
        }
        case 160: {
          ShowRollingMessage = input.ReadBool();
          break;
        }
        case 168: {
          KeepCurrentMessage = input.ReadBool();
          break;
        }
        case 176: {
          CurrentMessagePosX = input.ReadInt32();
          break;
        }
        case 184: {
          CurrentMessagePosY = input.ReadInt32();
          break;
        }
        case 194: {
          Title = input.ReadString();
          break;
        }
      }
    }
#endif
        }

#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input)
        {
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (tag)
                {
                    default:
                        _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
                        break;
                    case 10:
                    {
                        Name = input.ReadString();
                        break;
                    }
                    case 16:
                    {
                        DontDestory = input.ReadBool();
                        break;
                    }
                    case 24:
                    {
                        SortingOrder = input.ReadInt32();
                        break;
                    }
                    case 32:
                    {
                        FullScreen = input.ReadBool();
                        break;
                    }
                    case 40:
                    {
                        AllowActorControl = input.ReadBool();
                        break;
                    }
                    case 48:
                    {
                        InstructionOffsetX = input.ReadInt32();
                        break;
                    }
                    case 56:
                    {
                        InstructionOffsetY = input.ReadInt32();
                        break;
                    }
                    case 64:
                    {
                        StatusBarStyle = input.ReadInt32();
                        break;
                    }
                    case 72:
                    {
                        StatusBarRightPadding = input.ReadInt32();
                        break;
                    }
                    case 90:
                    {
                        BgmCue = input.ReadString();
                        break;
                    }
                    case 98:
                    {
                        BgmBlock = input.ReadString();
                        break;
                    }
                    case 104:
                    {
                        RevertPreviousBgm = input.ReadBool();
                        break;
                    }
                    case 112:
                    {
                        Penetrate = input.ReadBool();
                        break;
                    }
                    case 122:
                    {
                        OpenSE = input.ReadString();
                        break;
                    }
                    case 130:
                    {
                        CloseSE = input.ReadString();
                        break;
                    }
                    case 136:
                    {
                        BgmCut = input.ReadInt32();
                        break;
                    }
                    case 160:
                    {
                        ShowRollingMessage = input.ReadBool();
                        break;
                    }
                    case 168:
                    {
                        KeepCurrentMessage = input.ReadBool();
                        break;
                    }
                    case 176:
                    {
                        CurrentMessagePosX = input.ReadInt32();
                        break;
                    }
                    case 184:
                    {
                        CurrentMessagePosY = input.ReadInt32();
                        break;
                    }
                    case 194:
                    {
                        Title = input.ReadString();
                        break;
                    }
                }
            }
        }
#endif
    }
}