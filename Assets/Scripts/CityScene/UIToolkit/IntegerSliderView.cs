using System;
using System.Globalization;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public sealed class IntegerSliderView : VisualElement, IDisposable
    {
        const string RootClass = "integer-slider";
        const string DecreaseButtonName = "IntegerSliderDecrease";
        const string DecreaseButtonClass = "integer-slider__decrease";
        const string CenterName = "IntegerSliderCenter";
        const string CenterClass = "integer-slider__center";
        const string SliderName = "IntegerSlider";
        const string SliderClass = "integer-slider__slider";
        const string SliderDraggerClass = "unity-base-slider__dragger";
        const string IncreaseButtonName = "IntegerSliderIncrease";
        const string IncreaseButtonClass = "integer-slider__increase";
        const string ArrowIconClass = "integer-slider__arrow-icon";
        const string ValueLabelName = "IntegerSliderValue";
        const string ValueLabelClass = "integer-slider__value";

        readonly Button decreaseButton;
        readonly Button increaseButton;
        readonly VisualElement decreaseIcon;
        readonly VisualElement increaseIcon;
        readonly VisualElement centerRoot;
        readonly SliderInt slider;
        readonly Label valueLabel;

        int rangeLowerLimit;
        int rangeUpperLimit;
        int lowerLimit;
        int upperLimit;
        int currentValue;
        int currentSliderPosition;
        int leftStepCount;
        int rightStepCount;
        int sliderHalfRange;

        public event Action<int> ValueChanged;

        public IntegerSliderView()
            : this(-1, 1)
        {
        }

        public IntegerSliderView(int lowerLimit, int upperLimit)
        {
            AddToClassList(RootClass);

            decreaseButton = new Button
            {
                name = DecreaseButtonName,
                text = string.Empty,
                pickingMode = PickingMode.Position
            };
            decreaseButton.AddToClassList(DecreaseButtonClass);

            decreaseIcon = new VisualElement
            {
                name = "IntegerSliderDecreaseIcon",
                pickingMode = PickingMode.Ignore
            };
            decreaseIcon.AddToClassList(ArrowIconClass);
            decreaseButton.Add(decreaseIcon);

            centerRoot = new VisualElement
            {
                name = CenterName,
                pickingMode = PickingMode.Position
            };
            centerRoot.AddToClassList(CenterClass);

            slider = new SliderInt
            {
                name = SliderName,
                showInputField = false,
                pickingMode = PickingMode.Position
            };
            slider.AddToClassList(SliderClass);
            centerRoot.Add(slider);

            valueLabel = new Label
            {
                name = ValueLabelName,
                pickingMode = PickingMode.Ignore
            };
            valueLabel.AddToClassList(ValueLabelClass);
            centerRoot.Add(valueLabel);

            increaseButton = new Button
            {
                name = IncreaseButtonName,
                text = string.Empty,
                pickingMode = PickingMode.Position
            };
            increaseButton.AddToClassList(IncreaseButtonClass);

            increaseIcon = new VisualElement
            {
                name = "IntegerSliderIncreaseIcon",
                pickingMode = PickingMode.Ignore
            };
            increaseIcon.AddToClassList(ArrowIconClass);
            increaseIcon.style.rotate = new Rotate(new Angle(180f, AngleUnit.Degree));
            increaseButton.Add(increaseIcon);

            centerRoot.Add(decreaseButton);
            centerRoot.Add(increaseButton);
            Add(slider);
            Add(centerRoot);

            SetRange(lowerLimit, upperLimit);
            SetLimits(lowerLimit, upperLimit);
            SetValueWithoutNotify(0);

            decreaseButton.clicked += HandleDecreaseClicked;
            increaseButton.clicked += HandleIncreaseClicked;
            RegisterCallback<AttachToPanelEvent>(_ => AttachCenterToDragger());
            slider.RegisterCallback<GeometryChangedEvent>(_ => AttachCenterToDragger());
            slider.RegisterValueChangedCallback(HandleSliderValueChanged);
        }

        public int LowerLimit
        {
            get => lowerLimit;
            set => SetLimits(value, upperLimit);
        }

        public int UpperLimit
        {
            get => upperLimit;
            set => SetLimits(lowerLimit, value);
        }

        public int Value => currentValue;

        public void SetRange(int newLowerLimit, int newUpperLimit)
        {
            if (newLowerLimit > newUpperLimit)
            {
                int oldLowerLimit = newLowerLimit;
                newLowerLimit = newUpperLimit;
                newUpperLimit = oldLowerLimit;
            }

            rangeLowerLimit = Math.Min(newLowerLimit, 0);
            rangeUpperLimit = Math.Max(newUpperLimit, 0);

            leftStepCount = Math.Abs(rangeLowerLimit);
            rightStepCount = rangeUpperLimit;
            sliderHalfRange = CalculateSliderHalfRange(leftStepCount, rightStepCount);

            slider.UnregisterValueChangedCallback(HandleSliderValueChanged);
            slider.lowValue = -sliderHalfRange;
            slider.highValue = sliderHalfRange;

            currentSliderPosition = MapValueToSliderPosition(currentValue);
            slider.SetValueWithoutNotify(currentSliderPosition);

            slider.RegisterValueChangedCallback(HandleSliderValueChanged);
            SetValueWithoutNotify(currentValue);
        }

        public void SetLimits(int newLowerLimit, int newUpperLimit)
        {
            if (newLowerLimit > newUpperLimit)
            {
                int oldLowerLimit = newLowerLimit;
                newLowerLimit = newUpperLimit;
                newUpperLimit = oldLowerLimit;
            }

            lowerLimit = Math.Max(rangeLowerLimit, Math.Min(newLowerLimit, 0));
            upperLimit = Math.Min(rangeUpperLimit, Math.Max(newUpperLimit, 0));

            SetValueWithoutNotify(ClampValue(currentValue));
        }

        public void Dispose()
        {
            decreaseButton.clicked -= HandleDecreaseClicked;
            increaseButton.clicked -= HandleIncreaseClicked;
            slider.UnregisterValueChangedCallback(HandleSliderValueChanged);
            ValueChanged = null;
        }

        void HandleDecreaseClicked()
        {
            Step(-1);
        }

        void HandleIncreaseClicked()
        {
            Step(1);
        }

        void Step(int delta)
        {
            int nextValue = ClampValue(currentValue + delta);
            if (nextValue == currentValue)
            {
                ApplyValueLabel();
                return;
            }

            currentValue = nextValue;
            currentSliderPosition = MapValueToSliderPosition(currentValue);
            slider.SetValueWithoutNotify(currentSliderPosition);
            ApplyValueLabel();
            ValueChanged?.Invoke(currentValue);
        }

        void HandleSliderValueChanged(ChangeEvent<int> evt)
        {
            int nextValue = currentValue;
            if (evt.newValue > currentSliderPosition)
            {
                nextValue = ClampValue(currentValue + 1);
            }
            else if (evt.newValue < currentSliderPosition)
            {
                nextValue = ClampValue(currentValue - 1);
            }

            if (nextValue == currentValue)
            {
                slider.SetValueWithoutNotify(currentSliderPosition);
                return;
            }

            currentValue = nextValue;
            currentSliderPosition = MapValueToSliderPosition(currentValue);
            slider.SetValueWithoutNotify(currentSliderPosition);
            ApplyValueLabel();
            ValueChanged?.Invoke(currentValue);
        }

        void SetValueWithoutNotify(int nextValue)
        {
            currentValue = ClampValue(nextValue);
            currentSliderPosition = MapValueToSliderPosition(currentValue);
            slider.SetValueWithoutNotify(currentSliderPosition);
            ApplyValueLabel();
        }

        int MapValueToSliderPosition(int value)
        {
            if (value < 0)
            {
                if (leftStepCount <= 0)
                {
                    return 0;
                }

                return -ToScaledSliderPosition(-value, leftStepCount);
            }

            if (value > 0)
            {
                if (rightStepCount <= 0)
                {
                    return 0;
                }

                return ToScaledSliderPosition(value, rightStepCount);
            }

            return 0;
        }

        int ToScaledSliderPosition(int value, int stepCount)
        {
            return (int)((long)sliderHalfRange * value / stepCount);
        }

        int ClampValue(int targetValue)
        {
            if (targetValue < rangeLowerLimit)
            {
                targetValue = rangeLowerLimit;
            }

            if (targetValue > rangeUpperLimit)
            {
                targetValue = rangeUpperLimit;
            }

            if (targetValue < lowerLimit)
            {
                return lowerLimit;
            }

            if (targetValue > upperLimit)
            {
                return upperLimit;
            }

            return targetValue;
        }

        void ApplyValueLabel()
        {
            valueLabel.text = currentValue.ToString(CultureInfo.InvariantCulture);
            decreaseButton.SetEnabled(currentValue > lowerLimit);
            increaseButton.SetEnabled(currentValue < upperLimit);
            AttachCenterToDragger();
        }

        void AttachCenterToDragger()
        {
            VisualElement dragger = slider.Q(className: SliderDraggerClass);
            if (dragger == null || centerRoot.parent == dragger)
            {
                return;
            }

            centerRoot.RemoveFromHierarchy();
            dragger.Add(centerRoot);
        }

        static int CalculateSliderHalfRange(int leftSteps, int rightSteps)
        {
            if (leftSteps <= 0 && rightSteps <= 0)
            {
                return 1;
            }

            if (leftSteps <= 0)
            {
                return Math.Max(1, rightSteps);
            }

            if (rightSteps <= 0)
            {
                return Math.Max(1, leftSteps);
            }

            long gcd = GreatestCommonDivisor(leftSteps, rightSteps);
            long lcm = (long)leftSteps / gcd * rightSteps;
            if (lcm > int.MaxValue)
            {
                return Math.Max(leftSteps, rightSteps);
            }

            return (int)Math.Max(1, lcm);
        }

        static long GreatestCommonDivisor(int left, int right)
        {
            long a = Math.Abs((long)left);
            long b = Math.Abs((long)right);

            while (b != 0)
            {
                long remainder = a % b;
                a = b;
                b = remainder;
            }

            return a == 0 ? 1 : a;
        }
    }
}
