﻿namespace VisualPlus.Controls
{
    #region Namespace

    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;

    using VisualPlus.Enums;
    using VisualPlus.Framework;
    using VisualPlus.Framework.GDI;
    using VisualPlus.Framework.Structure;
    using VisualPlus.Localization;

    #endregion

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(Button))]
    [Designer(VSDesignerBinding.VisualButton)]
    [Description("The VisualButton")]
    [DefaultEvent("Click")]
    public sealed class VisualButton : Button
    {
        #region Variables

        private bool animation = true;
        private Border border = new Border();

        private Color[] buttonDisabled =
            {
                Settings.DefaultValue.Style.ControlDisabled,
                ControlPaint.Light(Settings.DefaultValue.Style.ControlDisabled),
                Settings.DefaultValue.Style.ControlDisabled
            };

        private Color[] buttonHover =
            {
                Settings.DefaultValue.Style.ButtonHoverColor,
                ControlPaint.Light(Settings.DefaultValue.Style.ButtonHoverColor),
                Settings.DefaultValue.Style.ButtonHoverColor
            };

        private Color[] buttonNormal =
            {
                Settings.DefaultValue.Style.ButtonNormalColor,
                ControlPaint.Light(Settings.DefaultValue.Style.ButtonNormalColor),
                Settings.DefaultValue.Style.ButtonNormalColor
            };

        private Color[] buttonPressed =
            {
                Settings.DefaultValue.Style.ButtonDownColor,
                ControlPaint.Light(Settings.DefaultValue.Style.ButtonDownColor),
                Settings.DefaultValue.Style.ButtonDownColor
            };

        private GraphicsPath controlGraphicsPath;
        private ControlState controlState = ControlState.Normal;

        private VFXManager effectsManager;
        private Point endPoint;
        private SizeF fontSize;
        private Color foreColor = Settings.DefaultValue.Style.ForeColor(0);

        private float gradientAngle;

        private LinearGradientBrush gradientBrush;

        private float[] gradientPosition = { 0, 1 / 2f, 1 };

        private VFXManager hoverEffectsManager;
        private Image icon;
        private bool iconBorder;
        private GraphicsPath iconGraphicsPath;
        private Point iconPoint = new Point(0, 0);
        private Rectangle iconRectangle;
        private Size iconSize = new Size(24, 24);

        private Point startPoint;
        private Rectangle textboxRectangle;
        private Color textDisabledColor = Settings.DefaultValue.Style.TextDisabled;
        private TextImageRelation textImageRelation = TextImageRelation.Overlay;
        private Point textPoint = new Point(0, 0);
        private TextRenderingHint textRendererHint = Settings.DefaultValue.TextRenderingHint;
        private VisualStylesManager visualStylesManager;

        #endregion

        #region Constructors

        public VisualButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor,
                true);

            UpdateStyles();

            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AutoSize = false;
            Margin = new Padding(4, 6, 4, 6);
            Padding = new Padding(0);
            Size = new Size(140, 45);
            BackColor = Color.Transparent;

            Font = new Font(Settings.DefaultValue.Style.FontFamily, Font.Size);

            // Setup effects animation
            effectsManager = new VFXManager(false)
                {
                    Increment = 0.03,
                    EffectType = EffectType.EaseOut
                };
            hoverEffectsManager = new VFXManager
                {
                    Increment = 0.07,
                    EffectType = EffectType.Linear
                };

            hoverEffectsManager.OnAnimationProgress += sender => Invalidate();
            effectsManager.OnAnimationProgress += sender => Invalidate();
        }

        #endregion

        #region Properties

        [DefaultValue(Settings.DefaultValue.Animation)]
        [Category(Localize.Category.Behavior)]
        [Description(Localize.Description.Animation)]
        public bool Animation
        {
            get
            {
                return animation;
            }

            set
            {
                animation = value;
                AutoSize = AutoSize; // Make AutoSize directly set the bounds.

                if (value)
                {
                    Margin = new Padding(0);
                }

                Invalidate();
            }
        }

        [TypeConverter(typeof(BorderConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Localize.Category.Appearance)]
        public Border Border
        {
            get
            {
                return border;
            }

            set
            {
                border = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.ControlDisabled)]
        public Color[] DisabledColor
        {
            get
            {
                return buttonDisabled;
            }

            set
            {
                buttonDisabled = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Behavior)]
        [Description(Localize.Description.Angle)]
        public float GradientAngle
        {
            get
            {
                return gradientAngle;
            }

            set
            {
                gradientAngle = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.GradientPosition)]
        public float[] GradientPosition
        {
            get
            {
                return gradientPosition;
            }

            set
            {
                gradientPosition = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.HoverColor)]
        public Color[] HoverColor
        {
            get
            {
                return buttonHover;
            }

            set
            {
                buttonHover = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.Icon)]
        public Image Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.BorderVisible)]
        public bool IconBorder
        {
            get
            {
                return iconBorder;
            }

            set
            {
                iconBorder = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Layout)]
        [Description(Localize.Description.IconSize)]
        public Size IconSize
        {
            get
            {
                return iconSize;
            }

            set
            {
                iconSize = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        public Point MouseLocation { get; set; }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.NormalColor)]
        public Color[] NormalColor
        {
            get
            {
                return buttonNormal;
            }

            set
            {
                buttonNormal = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.PressedColor)]
        public Color[] PressedColor
        {
            get
            {
                return buttonPressed;
            }

            set
            {
                buttonPressed = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Design)]
        [Description(Localize.Description.Style)]
        public VisualStylesManager StyleManager
        {
            get
            {
                return visualStylesManager;
            }

            set
            {
                visualStylesManager = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.TextColor)]
        public Color TextColor
        {
            get
            {
                return foreColor;
            }

            set
            {
                foreColor = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.ComponentColor)]
        public Color TextDisabledColor
        {
            get
            {
                return textDisabledColor;
            }

            set
            {
                textDisabledColor = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Behavior)]
        [Description(Localize.Description.TextImageRelation)]
        public new TextImageRelation TextImageRelation
        {
            get
            {
                return textImageRelation;
            }

            set
            {
                textImageRelation = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.TextRenderingHint)]
        public TextRenderingHint TextRendering
        {
            get
            {
                return textRendererHint;
            }

            set
            {
                textRendererHint = value;
                Invalidate();
            }
        }

        #endregion

        #region Events

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode)
            {
                return;
            }

            controlState = ControlState.Normal;
            MouseEnter += (sender, args) =>
                {
                    controlState = ControlState.Hover;
                    hoverEffectsManager.StartNewAnimation(AnimationDirection.In);
                    Invalidate();
                };
            MouseLeave += (sender, args) =>
                {
                    controlState = ControlState.Normal;
                    hoverEffectsManager.StartNewAnimation(AnimationDirection.Out);
                    Invalidate();
                };
            MouseDown += (sender, args) =>
                {
                    if (args.Button == MouseButtons.Left)
                    {
                        controlState = ControlState.Down;

                        effectsManager.StartNewAnimation(AnimationDirection.In, args.Location);
                        Invalidate();
                    }
                };
            MouseUp += (sender, args) =>
                {
                    controlState = ControlState.Hover;

                    Invalidate();
                };
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            controlState = ControlState.Down;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            controlState = ControlState.Hover;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            controlState = ControlState.Normal;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            controlState = ControlState.Hover;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.Clear(Parent.BackColor);
            graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = textRendererHint;

            // Set control state color
            foreColor = Enabled ? foreColor : textDisabledColor;
            var controlTempColor = Enabled ? buttonNormal : buttonDisabled;

            // Gets the font size rectangle.
            fontSize = graphics.MeasureString(Text, Font);

            startPoint = new Point(ClientRectangle.Width, 0);
            endPoint = new Point(ClientRectangle.Width, ClientRectangle.Height);

            textPoint = GDI.ApplyTextImageRelation(graphics, textImageRelation, iconRectangle, Text, Font, ClientRectangle, false);
            textboxRectangle.Location = textPoint;
            iconPoint = GDI.ApplyTextImageRelation(graphics, textImageRelation, iconRectangle, Text, Font, ClientRectangle, true);
            iconRectangle = new Rectangle(iconPoint, iconSize);

            iconGraphicsPath = new GraphicsPath();
            iconGraphicsPath.AddRectangle(iconRectangle);
            iconGraphicsPath.CloseAllFigures();

            controlGraphicsPath = GDI.GetBorderShape(ClientRectangle, border.Shape, border.Rounding);

            // Draw control state
            if (Enabled)
            {
                // Button back color
                switch (controlState)
                {
                    case ControlState.Normal:
                        {
                            controlTempColor = buttonNormal;
                            break;
                        }

                    case ControlState.Hover:
                        {
                            controlTempColor = buttonHover;
                            break;
                        }

                    case ControlState.Down:
                        {
                            controlTempColor = buttonPressed;
                            break;
                        }

                    default:
                        {
                            controlTempColor = buttonNormal;
                            break;
                        }
                }
            }

            gradientBrush = GDI.CreateGradientBrush(controlTempColor, gradientPosition, gradientAngle, startPoint, endPoint);

            // Draw button background
            graphics.FillPath(gradientBrush, controlGraphicsPath);

            // Setup button border
            if (border.Visible)
            {
                GDI.DrawBorderType(graphics, controlState, controlGraphicsPath, border.Thickness, border.Color, border.HoverColor, border.HoverVisible);
            }

            if (string.IsNullOrEmpty(Text))
            {
                // Center Icon
                iconRectangle.X += 2;
                iconRectangle.Y += 2;
            }

            if (Icon != null)
            {
                // Update point
                iconRectangle.Location = iconPoint;

                // Draw icon border
                if (iconBorder)
                {
                    graphics.DrawPath(new Pen(border.Color), iconGraphicsPath);
                }

                // Draw icon
                graphics.DrawImage(Icon, iconRectangle);
            }

            // Draw string
            graphics.DrawString(Text, Font, new SolidBrush(foreColor), textboxRectangle);

            // Ripple
            if (effectsManager.IsAnimating() && animation)
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                for (var i = 0; i < effectsManager.GetAnimationCount(); i++)
                {
                    double animationValue = effectsManager.GetProgress(i);
                    Point animationSource = effectsManager.GetSource(i);

                    using (Brush rippleBrush = new SolidBrush(Color.FromArgb((int)(101 - animationValue * 100), Color.Black)))
                    {
                        var rippleSize = (int)(animationValue * Width * 2);
                        graphics.SetClip(controlGraphicsPath);
                        graphics.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                    }
                }

                graphics.SmoothingMode = SmoothingMode.None;
            }
        }

        #endregion
    }
}