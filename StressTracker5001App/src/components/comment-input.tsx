import { MicIcon, MicOffIcon, SendIcon } from "lucide-react";
import { useEffect, useRef } from "react";
import SpeechRecognition, {
  useSpeechRecognition,
} from "react-speech-recognition";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupTextarea,
} from "./ui/input-group";
import { Tooltip, TooltipContent, TooltipTrigger } from "./ui/tooltip";

interface CommentInputProps
  extends Omit<
    React.ComponentProps<typeof InputGroupTextarea>,
    "value" | "onChange"
  > {
  value: string;
  onChange: (value: string) => void;
}

export function CommentInput({ value, onChange, ...props }: CommentInputProps) {
  const ref = useRef<HTMLTextAreaElement>(null);
  const baseTextRef = useRef<string>("");

  const {
    transcript,
    listening,
    resetTranscript,
    browserSupportsSpeechRecognition,
  } = useSpeechRecognition();

  function handleStartVoiceToText() {
    // Store the current text before starting
    baseTextRef.current = value;
    resetTranscript();
    SpeechRecognition.startListening({
      continuous: true,
      language: "en-US",
      interimResults: true,
    });
  }

  // Update the input with live transcript
  useEffect(() => {
    if (listening) {
      const combinedText =
        baseTextRef.current && transcript
          ? `${baseTextRef.current} ${transcript}`.trim()
          : baseTextRef.current || transcript;
      onChange(combinedText);
    }
  }, [transcript, listening, onChange]);

  function handleStopVoiceToText() {
    SpeechRecognition.stopListening();
    // The final state is already in the input from the last transcript update
    // Just reset for next session
    setTimeout(() => resetTranscript(), 100);
  }

  const SpeechRecognitionIcon = listening ? MicOffIcon : MicIcon;
  const speechRecognitionAction = listening
    ? handleStopVoiceToText
    : handleStartVoiceToText;
  const speechRecognitionTooltip = listening
    ? "Stop recording"
    : "Start recording";

  return (
    <InputGroup onClick={() => ref.current?.focus()}>
      <InputGroupTextarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        {...props}
        ref={ref}
        placeholder="Comment..."
      />
      <InputGroupAddon align="block-end">
        {/* <Tooltip>
          <TooltipTrigger asChild>
            <InputGroupButton
              variant="outline"
              className="rounded-full"
              size="icon-xs"
            >
              <PlusIcon />
              <span className="sr-only">Attach a file</span>
            </InputGroupButton>
          </TooltipTrigger>
          <TooltipContent>
            <p>Attach a file</p>
          </TooltipContent>
        </Tooltip> */}

        {browserSupportsSpeechRecognition && (
          <Tooltip>
            <TooltipTrigger asChild>
              <InputGroupButton
                variant="outline"
                className="rounded-full"
                size="icon-xs"
                onClick={speechRecognitionAction}
              >
                <SpeechRecognitionIcon />
                <span className="sr-only">{speechRecognitionTooltip}</span>
              </InputGroupButton>
            </TooltipTrigger>
            <TooltipContent>
              <p>{speechRecognitionTooltip}</p>
            </TooltipContent>
          </Tooltip>
        )}

        <Tooltip>
          <TooltipTrigger asChild>
            <InputGroupButton
              variant="default"
              className="ml-auto rounded-full"
              size="icon-xs"
            >
              <SendIcon className="size-4" />
              <span className="sr-only">Comment</span>
            </InputGroupButton>
          </TooltipTrigger>
          <TooltipContent>
            <p>Comment...</p>
          </TooltipContent>
        </Tooltip>
      </InputGroupAddon>
    </InputGroup>
  );
}
