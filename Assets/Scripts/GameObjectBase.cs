using UnityEngine;

public class GameObjectBase : MonoBehaviour
{
    protected enum States
    {
        READY,
        ACTING,
        PAUSING,
        FINISHED
    }
    protected States state = States.READY;

    private float previous_timescale = 1.0f;

    protected virtual void Update()
    {
        if (IsPaused())
        {
            TransitionToPausing();
        }
        else if (IsUnpaused())
        {
            TransitionToActing();
        }

        switch (state)
        {
            case States.READY:
                UpdateOnReady();
                break;
            case States.ACTING:
                UpdateOnActing();
                break;
            case States.PAUSING:
                UpdateOnPausing();
                break;
            case States.FINISHED:
                UpdateOnFinished();
                break;
        }

        previous_timescale = Time.timeScale;
    }

    private bool IsPaused()
    {
        const float STOP_TIME_BORDER = 0.5f;
        float current_timescale = Time.timeScale;
        if (current_timescale < STOP_TIME_BORDER && previous_timescale > STOP_TIME_BORDER)
        {
            return true;
        }
        return false;
    }

    private bool IsUnpaused()
    {
        const float STOP_TIME_BORDER = 0.5f;
        float current_timescale = Time.timeScale;
        if (current_timescale > STOP_TIME_BORDER && previous_timescale < STOP_TIME_BORDER)
        {
            return true;
        }
        return false;
    }

    protected virtual void UpdateOnReady()
    {
        return;
    }

    protected virtual void UpdateOnActing()
    {
        return;
    }

    protected virtual void UpdateOnPausing()
    {
        return;
    }

    protected virtual void UpdateOnFinished()
    {
        return;
    }


    protected void TransitionToReady()
    {
        state = States.READY;
        return;
    }

    protected void TransitionToActing()
    {
        state = States.ACTING;
        return;
    }

    protected void TransitionToPausing()
    {
        state = States.PAUSING;
        return;
    }

    protected void TransitionToFinished()
    {
        state = States.FINISHED;
        return;
    }
}
