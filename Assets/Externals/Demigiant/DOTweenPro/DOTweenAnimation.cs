// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/12 15:55

using System;
using System.Collections.Generic;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
#if true // UI_MARKER
using UnityEngine.UI;
#endif
#if false // TEXTMESHPRO_MARKER
using TMPro;
#endif

#pragma warning disable 1591
namespace DG.Tweening
{
    public enum TweenAnimationType
    {
        None,
        Move,
        LocalMove,
        Rotate,
        LocalRotate,
        Scale,
        Color,
        Fade,
        Text,
        PunchPosition,
        PunchRotation,
        PunchScale,
        ShakePosition,
        ShakeRotation,
        ShakeScale,
        CameraAspect,
        CameraBackgroundColor,
        CameraFieldOfView,
        CameraOrthoSize,
        CameraPixelRect,
        CameraRect,
        UIWidthHeight,
        UIAnchoredPosition,
        UIAnchoredPositionX,
        UIAnchoredPositionY,
    }
    
    /// <summary>
    /// Attach this to a GameObject to create a tween
    /// </summary>
    [AddComponentMenu("DOTween/DOTween Animation")]
    public class DOTweenAnimation : ABSAnimationComponent
    {
        #region EVENTS - EDITOR-ONLY

        /// <summary>Used internally by the editor</summary>
        public static event Action<DOTweenAnimation> OnReset;
        static void Dispatch_OnReset(DOTweenAnimation anim) { if (OnReset != null) OnReset(anim); }

        #endregion

        public bool targetIsSelf = true; // If FALSE allows to set the target manually
        public GameObject targetGO = null; // Used in case targetIsSelf is FALSE
        // If TRUE always uses the GO containing this DOTweenAnimation (and not the one containing the target) as DOTween's SetTarget target
        public bool tweenTargetIsTargetGO = true;

        public float delay;
        public float duration = 1;
        public Ease easeType = Ease.OutQuad;
        public AnimationCurve easeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public LoopType loopType = LoopType.Restart;
        public int loops = 1;
        public string id = "";
        public bool isRelative;
        public bool isFrom;
        public bool isIndependentUpdate = false;
        public bool autoKill = true;

        public bool isActive = true;
        public bool isValid;
        public Component target;
        public TweenAnimationType animationType;
        public TargetType targetType;
        public TargetType forcedTargetType; // Used when choosing between multiple targets
        public bool autoPlay = true;
        public bool useTargetAsV3;

        public float endValueFloat;
        public Vector3 endValueV3;
        public Vector2 endValueV2;
        public Color endValueColor = new Color(1, 1, 1, 1);
        public string endValueString = "";
        public Rect endValueRect = new Rect(0, 0, 0, 0);
        public Transform endValueTransform;

        public bool fromCurrent = true;
        public float fromValueFloat;
        public Vector3 fromValueV3;
        public Vector2 fromValueV2;
        public Color fromValueColor = new Color(1, 1, 1, 1);
        public string fromValueString = "";
        public Rect fromValueRect = new Rect(0, 0, 0, 0);
        public Transform fromValueTransform;

        public bool optionalBool0;
        public float optionalFloat0;
        public int optionalInt0;
        public RotateMode optionalRotationMode = RotateMode.Fast;
        public ScrambleMode optionalScrambleMode = ScrambleMode.None;
        public string optionalString;

        bool _tweenCreated; // TRUE after the tweens have been created
        int _playCount = -1; // Used when calling DOPlayNext

        #region Unity Methods

        void Awake()
        {
            if (!isActive || !isValid) return;

            if (animationType != TweenAnimationType.Move || !useTargetAsV3) {
                // Don't create tweens if we're using a RectTransform as a Move target,
                // because that will work only inside Start
                CreateTween();
                _tweenCreated = true;
            }
        }

        void Start()
        {
            if (_tweenCreated || !isActive || !isValid) return;

            CreateTween();
            _tweenCreated = true;
        }

        void Reset()
        {
            Dispatch_OnReset(this);
        }

        void OnDestroy()
        {
            if (tween != null && tween.IsActive()) tween.Kill();
            tween = null;
        }

        // Used also by DOTweenAnimationInspector when applying runtime changes and restarting
        public void CreateTween()
        {
//            if (target == null) {
//                Debug.LogWarning(string.Format("{0} :: This DOTweenAnimation's target is NULL, because the animation was created with a DOTween Pro version older than 0.9.255. To fix this, exit Play mode then simply select this object, and it will update automatically", this.gameObject.name), this.gameObject);
//                return;
//            }

            GameObject tweenGO = GetTweenGO();
            if (target == null || tweenGO == null) {
                if (targetIsSelf && target == null) {
                    // Old error caused during upgrade from DOTween Pro 0.9.255
                    Debug.LogWarning(string.Format("{0} :: This DOTweenAnimation's target is NULL, because the animation was created with a DOTween Pro version older than 0.9.255. To fix this, exit Play mode then simply select this object, and it will update automatically", this.gameObject.name), this.gameObject);
                } else {
                    // Missing non-self target
                    Debug.LogWarning(string.Format("{0} :: This DOTweenAnimation's target/GameObject is unset: the tween will not be created.", this.gameObject.name), this.gameObject);
                }
                return;
            }

            if (forcedTargetType != TargetType.Unset) targetType = forcedTargetType;
            if (targetType == TargetType.Unset) {
                // Legacy DOTweenAnimation (made with a version older than 0.9.450) without stored targetType > assign it now
                targetType = TypeToDOTargetType(target.GetType());
            }

            TweenerCore<Vector3, Vector3, VectorOptions> tweenV3 = null;
            TweenerCore<Quaternion, Vector3, QuaternionOptions> tweenQ = null;
            TweenerCore<Vector2, Vector2, VectorOptions> tweenV2 = null;
            TweenerCore<Color, Color, ColorOptions> tweenColor = null;
            TweenerCore<float, float, FloatOptions> tweenFloat = null;
            TweenerCore<Rect, Rect, RectOptions> tweenRect = null;

            switch (animationType) {
            case TweenAnimationType.None:
                break;
            case TweenAnimationType.Move:
                if (useTargetAsV3) {
                    isRelative = false;
                    if (endValueTransform == null) {
                        Debug.LogWarning(string.Format("{0} :: This tween's TO target is NULL, a Vector3 of (0,0,0) will be used instead", this.gameObject.name), this.gameObject);
                        endValueV3 = Vector3.zero;
                    } else {
#if true // UI_MARKER
                        if (targetType == TargetType.RectTransform) {
                            RectTransform endValueT = endValueTransform as RectTransform;
                            if (endValueT == null) {
                                Debug.LogWarning(string.Format("{0} :: This tween's TO target should be a RectTransform, a Vector3 of (0,0,0) will be used instead", this.gameObject.name), this.gameObject);
                                endValueV3 = Vector3.zero;
                            } else {
                                RectTransform rTarget = target as RectTransform;
                                if (rTarget == null) {
                                    Debug.LogWarning(string.Format("{0} :: This tween's target and TO target are not of the same type. Please reassign the values", this.gameObject.name), this.gameObject);
                                } else {
                                    // Problem: doesn't work inside Awake (ararargh!)
                                    endValueV3 = DOTweenModuleUI.Utils.SwitchToRectTransform(endValueT, rTarget);
                                    if (!fromCurrent && fromValueTransform != null)
                                        fromValueV3 = DOTweenModuleUI.Utils.SwitchToRectTransform((RectTransform)fromValueTransform, rTarget);
                                }
                            }
                        } else
#endif
                            endValueV3 = endValueTransform.position;
                    }
                }
                switch (targetType) {
                case TargetType.Transform:
                    tweenV3 = ((Transform)target).DOMove(endValueV3, duration, optionalBool0);
                    tween = tweenV3;
                    break;
                case TargetType.RectTransform:
#if true // UI_MARKER
                    tweenV3 = ((RectTransform)target).DOAnchorPos3D(endValueV3, duration, optionalBool0);
#else
                    tweenV3 = ((Transform)target).DOMove(endValueV3, duration, optionalBool0);
#endif
                    tween = tweenV3;
                    break;
                case TargetType.Rigidbody:
#if true // PHYSICS_MARKER
                    tweenV3 = ((Rigidbody)target).DOMove(endValueV3, duration, optionalBool0);
#else
                    tweenV3 = ((Transform)target).DOMove(endValueV3, duration, optionalBool0);
#endif
                    tween = tweenV3;
                    break;
                case TargetType.Rigidbody2D:
#if false // PHYSICS2D_MARKER
                    tweenV3 = ((Rigidbody2D)target).DOMove(endValueV3, duration, optionalBool0);
#else
                    tweenV3 = ((Transform)target).DOMove(endValueV3, duration, optionalBool0);
#endif
                    tween = tweenV3;
                    break;
                }
                break;
            case TweenAnimationType.LocalMove:
                tweenV3 = tweenGO.transform.DOLocalMove(endValueV3, duration, optionalBool0);
                tween = tweenV3;
                break;
            case TweenAnimationType.Rotate:
                switch (targetType) {
                case TargetType.Transform:
                    tweenQ = ((Transform)target).DORotate(endValueV3, duration, optionalRotationMode);
                    tween = tweenQ;
                    break;
                case TargetType.Rigidbody:
#if true // PHYSICS_MARKER
                    tweenQ = ((Rigidbody)target).DORotate(endValueV3, duration, optionalRotationMode);
#else
                    tweenQ = ((Transform)target).DORotate(endValueV3, duration, optionalRotationMode);
#endif
                    tween = tweenQ;
                    break;
                case TargetType.Rigidbody2D:
#if false // PHYSICS2D_MARKER
                    tweenQ = ((Rigidbody2D)target).DORotate(endValueFloat, duration);
#else
                    tweenQ = ((Transform)target).DORotate(endValueV3, duration, optionalRotationMode);
#endif
                    tween = tweenQ;
                    break;
                }
                break;
            case TweenAnimationType.LocalRotate:
                tweenQ = tweenGO.transform.DOLocalRotate(endValueV3, duration, optionalRotationMode);
                tween = tweenQ;
                break;
            case TweenAnimationType.Scale:
                switch (targetType) {
#if false // TK2D_MARKER
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOScale(optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween = ((tk2dBaseSprite)target).DOScale(optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
                    break;
#endif
                default:
                    tweenV3 = tweenGO.transform.DOScale(optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
                    tween = tweenV3;
                    break;
                }
                break;
#if true // UI_MARKER
            case TweenAnimationType.UIWidthHeight:
                tweenV2 = ((RectTransform)target).DOSizeDelta(optionalBool0 ? new Vector2(endValueFloat, endValueFloat) : endValueV2, duration);
                tween = tweenV2;
                break;
            case TweenAnimationType.UIAnchoredPosition:
                tweenV2 = ((RectTransform)target).DOAnchorPos(endValueV2, duration);
                tween = tweenV2;
                break;
            case TweenAnimationType.UIAnchoredPositionX:
                tweenV2 = ((RectTransform)target).DOAnchorPosX(endValueFloat, duration);
                tween = tweenV2;
                break;
            case TweenAnimationType.UIAnchoredPositionY:
                tweenV2 = ((RectTransform)target).DOAnchorPosY(endValueFloat, duration);
                tween = tweenV2;
                break;
#endif
            case TweenAnimationType.Color:
                isRelative = false;
                switch (targetType) {
                case TargetType.Renderer:
                    tweenColor = ((Renderer)target).material.DOColor(endValueColor, duration);
                    tween = tweenColor;
                    break;
                case TargetType.Light:
                    tweenColor = ((Light)target).DOColor(endValueColor, duration);
                    tween = tweenColor;
                    break;
#if true // SPRITE_MARKER
                case TargetType.SpriteRenderer:
                    tweenColor = ((SpriteRenderer)target).DOColor(endValueColor, duration);
                    tween = tweenColor;
                    break;
#endif
#if true // UI_MARKER
                case TargetType.Image:
                    tweenColor = ((Graphic)target).DOColor(endValueColor, duration);
                    tween = tweenColor;
                    break;
                case TargetType.Text:
                    tweenColor = ((Text)target).DOColor(endValueColor, duration);
                    tween = tweenColor;
                    break;
#endif
#if false // TK2D_MARKER
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOColor(endValueColor, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween = ((tk2dBaseSprite)target).DOColor(endValueColor, duration);
                    break;
#endif
#if false // TEXTMESHPRO_MARKER
                case TargetType.TextMeshProUGUI:
                    tweenColor = ((TextMeshProUGUI)target).DOColor(endValueColor, duration);
                    tween = tweenColor;
                    break;
                case TargetType.TextMeshPro:
                    tweenColor = ((TextMeshPro)target).DOColor(endValueColor, duration);
                    tween = tweenColor;
                    break;
#endif
                }
                break;
            case TweenAnimationType.Fade:
                isRelative = false;
                switch (targetType) {
                case TargetType.Renderer:
                    tweenColor = ((Renderer)target).material.DOFade(endValueFloat, duration);
                    tween = tweenColor;
                    break;
                case TargetType.Light:
                    tweenFloat = ((Light)target).DOIntensity(endValueFloat, duration);
                    tween = tweenFloat;
                    break;
#if true // SPRITE_MARKER
                case TargetType.SpriteRenderer:
                    tweenColor = ((SpriteRenderer)target).DOFade(endValueFloat, duration);
                    tween = tweenColor;
                    break;
#endif
#if true // UI_MARKER
                case TargetType.Image:
                    tweenColor = ((Graphic)target).DOFade(endValueFloat, duration);
                    tween = tweenColor;
                    break;
                case TargetType.Text:
                    tweenColor = ((Text)target).DOFade(endValueFloat, duration);
                    tween = tweenColor;
                    break;
                case TargetType.CanvasGroup:
                    tweenFloat = ((CanvasGroup)target).DOFade(endValueFloat, duration);
                    tween = tweenFloat;
                    break;
#endif
#if false // TK2D_MARKER
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOFade(endValueFloat, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween = ((tk2dBaseSprite)target).DOFade(endValueFloat, duration);
                    break;
#endif
#if false // TEXTMESHPRO_MARKER
                case TargetType.TextMeshProUGUI:
                    tweenColor = ((TextMeshProUGUI)target).DOFade(endValueFloat, duration);
                    tween = tweenColor;
                    break;
                case TargetType.TextMeshPro:
                    tweenColor = ((TextMeshPro)target).DOFade(endValueFloat, duration);
                    tween = tweenColor;
                    break;
#endif
                }
                break;
            case TweenAnimationType.Text:
#if true // UI_MARKER
                switch (targetType) {
                case TargetType.Text:
                    tween = ((Text)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
                }
#endif
#if false // TK2D_MARKER
                switch (targetType) {
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
                }
#endif
#if false // TEXTMESHPRO_MARKER
                switch (targetType) {
                case TargetType.TextMeshProUGUI:
                    tween = ((TextMeshProUGUI)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
                case TargetType.TextMeshPro:
                    tween = ((TextMeshPro)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
                }
#endif
                break;
            case TweenAnimationType.PunchPosition:
                switch (targetType) {
                case TargetType.Transform:
                    tween = ((Transform)target).DOPunchPosition(endValueV3, duration, optionalInt0, optionalFloat0, optionalBool0);
                    break;
#if true // UI_MARKER
                case TargetType.RectTransform:
                    tween = ((RectTransform)target).DOPunchAnchorPos(endValueV3, duration, optionalInt0, optionalFloat0, optionalBool0);
                    break;
#endif
                }
                break;
            case TweenAnimationType.PunchScale:
                tween = tweenGO.transform.DOPunchScale(endValueV3, duration, optionalInt0, optionalFloat0);
                break;
            case TweenAnimationType.PunchRotation:
                tween = tweenGO.transform.DOPunchRotation(endValueV3, duration, optionalInt0, optionalFloat0);
                break;
            case TweenAnimationType.ShakePosition:
                switch (targetType) {
                case TargetType.Transform:
                    tween = ((Transform)target).DOShakePosition(duration, endValueV3, optionalInt0, optionalFloat0, optionalBool0);
                    break;
#if true // UI_MARKER
                case TargetType.RectTransform:
                    tween = ((RectTransform)target).DOShakeAnchorPos(duration, endValueV3, optionalInt0, optionalFloat0, optionalBool0);
                    break;
#endif
                }
                break;
            case TweenAnimationType.ShakeScale:
                tween = tweenGO.transform.DOShakeScale(duration, endValueV3, optionalInt0, optionalFloat0);
                break;
            case TweenAnimationType.ShakeRotation:
                tween = tweenGO.transform.DOShakeRotation(duration, endValueV3, optionalInt0, optionalFloat0);
                break;
            case TweenAnimationType.CameraAspect:
                tweenFloat = ((Camera)target).DOAspect(endValueFloat, duration);
                tween = tweenFloat;
                break;
            case TweenAnimationType.CameraBackgroundColor:
                tweenColor = ((Camera)target).DOColor(endValueColor, duration);
                tween = tweenColor;
                break;
            case TweenAnimationType.CameraFieldOfView:
                tweenFloat = ((Camera)target).DOFieldOfView(endValueFloat, duration);
                tween = tweenFloat;
                break;
            case TweenAnimationType.CameraOrthoSize:
                tweenFloat = ((Camera)target).DOOrthoSize(endValueFloat, duration);
                tween = tweenFloat;
                break;
            case TweenAnimationType.CameraPixelRect:
                tweenRect = ((Camera)target).DOPixelRect(endValueRect, duration);
                tween = tweenRect;
                break;
            case TweenAnimationType.CameraRect:
                tweenRect = ((Camera)target).DORect(endValueRect, duration);
                tween = tweenRect;
                break;
            }

            if (!fromCurrent && !isFrom)
            {
                if (optionalBool0)
                    tweenV3?.ChangeStartValue(new Vector3(fromValueFloat, fromValueFloat, fromValueFloat));
                else
                    tweenV3?.ChangeStartValue(fromValueV3);
                tweenQ?.ChangeStartValue(fromValueV3);
                tweenFloat?.ChangeStartValue(fromValueFloat);
                if (animationType == TweenAnimationType.Fade)
                    tweenColor?.ChangeStartValue(new Color(0.0f, 0.0f, 0.0f, fromValueFloat));
                else
                    tweenColor?.ChangeStartValue(fromValueColor);
                tweenRect?.ChangeStartValue(fromValueRect);
                if (animationType == TweenAnimationType.UIAnchoredPositionX)
                    tweenV2?.ChangeStartValue(new Vector2(fromValueFloat, 0));
                else if (animationType == TweenAnimationType.UIAnchoredPositionY)
                    tweenV2?.ChangeStartValue(new Vector2(0, fromValueFloat));
                else if (animationType == TweenAnimationType.UIWidthHeight && optionalBool0)
                    tweenV2?.ChangeStartValue(new Vector2(fromValueFloat, fromValueFloat));
                else
                    tweenV2?.ChangeStartValue(fromValueV2);
            }

            if (tween == null) return;

            if (isFrom) {
                ((Tweener)tween).From(isRelative);
            } else {
                tween.SetRelative(isRelative);
            }
            GameObject setTarget = targetIsSelf || !tweenTargetIsTargetGO ? this.gameObject : targetGO;
            tween.SetTarget(setTarget).SetDelay(delay).SetLoops(loops, loopType).SetAutoKill(autoKill)
                .OnKill(()=> tween = null);
            if (isSpeedBased) tween.SetSpeedBased();
            if (easeType == Ease.INTERNAL_Custom) tween.SetEase(easeCurve);
            else tween.SetEase(easeType);
            if (!string.IsNullOrEmpty(id)) tween.SetId(id);
            tween.SetUpdate(isIndependentUpdate);

            if (hasOnStart) {
                if (onStart != null) tween.OnStart(onStart.Invoke);
            } else onStart = null;
            if (hasOnPlay) {
                if (onPlay != null) tween.OnPlay(onPlay.Invoke);
            } else onPlay = null;
            if (hasOnUpdate) {
                if (onUpdate != null) tween.OnUpdate(onUpdate.Invoke);
            } else onUpdate = null;
            if (hasOnStepComplete) {
                if (onStepComplete != null) tween.OnStepComplete(onStepComplete.Invoke);
            } else onStepComplete = null;
            if (hasOnComplete) {
                if (onComplete != null) tween.OnComplete(onComplete.Invoke);
            } else onComplete = null;
            if (hasOnRewind) {
                if (onRewind != null) tween.OnRewind(onRewind.Invoke);
            } else onRewind = null;

            if (autoPlay) tween.Play();
            else tween.Pause();

            if (hasOnTweenCreated && onTweenCreated != null) onTweenCreated.Invoke();
        }

        #endregion

        #region Public Methods

        // These methods are here so they can be called directly via Unity's UGUI event system

        public override void DOPlay()
        {
            DOTween.Play(this.gameObject);
        }

        public override void DOPlayBackwards()
        {
            DOTween.PlayBackwards(this.gameObject);
        }

        public override void DOPlayForward()
        {
            DOTween.PlayForward(this.gameObject);
        }

        public override void DOPause()
        {
            DOTween.Pause(this.gameObject);
        }

        public override void DOTogglePause()
        {
            DOTween.TogglePause(this.gameObject);
        }

        public override void DORewind()
        {
        	_playCount = -1;
            // Rewind using Components order (in case there are multiple animations on the same property)
            DOTweenAnimation[] anims = this.gameObject.GetComponents<DOTweenAnimation>();
            for (int i = anims.Length - 1; i > -1; --i) {
                Tween t = anims[i].tween;
                if (t != null && t.IsInitialized()) anims[i].tween.Rewind();
            }
            // DOTween.Rewind(this.gameObject);
        }

        /// <summary>
        /// Restarts the tween
        /// </summary>
        public override void DORestart()
        { DORestart(false); }
        /// <summary>
        /// Restarts the tween
        /// </summary>
        /// <param name="fromHere">If TRUE, re-evaluates the tween's start and end values from its current position.
        /// Set it to TRUE when spawning the same DOTweenAnimation in different positions (like when using a pooling system)</param>
        public override void DORestart(bool fromHere)
        {
        	_playCount = -1;
            if (tween == null) {
                if (Debugger.logPriority > 1) Debugger.LogNullTween(tween); return;
            }
            if (fromHere && isRelative) ReEvaluateRelativeTween();
            DOTween.Restart(this.gameObject);
        }

        public override void DOComplete()
        {
            DOTween.Complete(this.gameObject);
        }

        public override void DOKill()
        {
            DOTween.Kill(this.gameObject);
            tween = null;
        }

        #region Specifics

        public void DOPlayById(string id)
        {
            DOTween.Play(this.gameObject, id);
        }
        public void DOPlayAllById(string id)
        {
            DOTween.Play(id);
        }

        public void DOPauseAllById(string id)
        {
            DOTween.Pause(id);
        }

        public void DOPlayBackwardsById(string id)
        {
            DOTween.PlayBackwards(this.gameObject, id);
        }
        public void DOPlayBackwardsAllById(string id)
        {
            DOTween.PlayBackwards(id);
        }

        public void DOPlayForwardById(string id)
        {
            DOTween.PlayForward(this.gameObject, id);
        }
        public void DOPlayForwardAllById(string id)
        {
            DOTween.PlayForward(id);
        }

        public void DOPlayNext()
        {
            DOTweenAnimation[] anims = this.GetComponents<DOTweenAnimation>();
            while (_playCount < anims.Length - 1) {
                _playCount++;
                DOTweenAnimation anim = anims[_playCount];
                if (anim != null && anim.tween != null && !anim.tween.IsPlaying() && !anim.tween.IsComplete()) {
                    anim.tween.Play();
                    break;
                }
            }
        }

        public void DORewindAndPlayNext()
        {
            _playCount = -1;
            DOTween.Rewind(this.gameObject);
            DOPlayNext();
        }

        public void DORewindAllById(string id)
        {
            _playCount = -1;
            DOTween.Rewind(id);
        }

        public void DORestartById(string id)
        {
            _playCount = -1;
            DOTween.Restart(this.gameObject, id);
        }
        public void DORestartAllById(string id)
        {
            _playCount = -1;
            DOTween.Restart(id);
        }

        /// <summary>
        /// Returns the tweens created by this DOTweenAnimation, in the same order as they appear in the Inspector (top to bottom)
        /// </summary>
        public List<Tween> GetTweens()
        {
//            return DOTween.TweensByTarget(this.gameObject);

            List<Tween> result = new List<Tween>();
            DOTweenAnimation[] anims = this.GetComponents<DOTweenAnimation>();
            foreach (DOTweenAnimation anim in anims) result.Add(anim.tween);
            return result;
        }

        #endregion

        #region Internal (also used by Inspector)

        public static TargetType TypeToDOTargetType(Type t)
        {
            string str = t.ToString();
            int dotIndex = str.LastIndexOf(".");
            if (dotIndex != -1) str = str.Substring(dotIndex + 1);
            if (str.IndexOf("Renderer") != -1 && (str != "SpriteRenderer")) str = "Renderer";
//#if true // PHYSICS_MARKER
//            if (str == "Rigidbody") str = "Transform";
//#endif
//#if true // PHYSICS2D_MARKER
//            if (str == "Rigidbody2D") str = "Transform";
//#endif
#if true // UI_MARKER
//            if (str == "RectTransform") str = "Transform";
            if (str == "RawImage") str = "Image"; // RawImages are managed like Images for DOTweenAnimation (color and fade use Graphic target anyway)
#endif
            return (TargetType)Enum.Parse(typeof(TargetType), str);
        }

        // Editor preview system
        /// <summary>
        /// Previews the tween in the editor. Only for DOTween internal usage: don't use otherwise.
        /// </summary>
        public Tween CreateEditorPreview()
        {
            if (Application.isPlaying) return null;

            CreateTween();
            return tween;
        }

        #endregion

        #endregion

        #region Private

        // Returns the gameObject whose target component should be animated
        GameObject GetTweenGO()
        {
            return targetIsSelf ? this.gameObject : targetGO;
        }

        // Re-evaluate relative position of path
        void ReEvaluateRelativeTween()
        {
            GameObject tweenGO = GetTweenGO();
            if (tweenGO == null) {
                Debug.LogWarning(string.Format("{0} :: This DOTweenAnimation's target/GameObject is unset: the tween will not be created.", this.gameObject.name), this.gameObject);
                return;
            }
            if (animationType == TweenAnimationType.Move) {
                ((Tweener)tween).ChangeEndValue(tweenGO.transform.position + endValueV3, true);
            } else if (animationType == TweenAnimationType.LocalMove) {
                ((Tweener)tween).ChangeEndValue(tweenGO.transform.localPosition + endValueV3, true);
            }
        }

        #endregion
    }

    public static class DOTweenAnimationExtensions
    {
//        // Doesn't work on Win 8.1
//        public static bool IsSameOrSubclassOf(this Type t, Type tBase)
//        {
//            return t.IsSubclassOf(tBase) || t == tBase;
//        }

        public static bool IsSameOrSubclassOf<T>(this Component t)
        {
            return t is T;
        }
    }
}
