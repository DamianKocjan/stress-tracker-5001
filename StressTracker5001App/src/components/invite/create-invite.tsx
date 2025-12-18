import { ROLE, type BoardMemberRoleDto } from "@/dto/board-member.dto";
import { useGenerateInviteMutation } from "@/hooks/use-generate-invite-mutation";
import { InviteSchema } from "@/schemas/invite";
import { useForm } from "@tanstack/react-form";
import { CheckIcon, CopyIcon, UserPlus2Icon } from "lucide-react";
import { useEffect, useState } from "react";
import { Button } from "../ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldSet,
} from "../ui/field";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { Popover, PopoverContent, PopoverTrigger } from "../ui/popover";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../ui/select";
import { Tooltip, TooltipContent, TooltipTrigger } from "../ui/tooltip";

interface CreateInviteProps {
  boardId: number;
}

export function CreateInvite({ boardId }: CreateInviteProps) {
  const {
    mutateAsync: generateInvite,
    data,
    reset,
  } = useGenerateInviteMutation(boardId);

  const [hasCopied, setHasCopied] = useState(false);

  useEffect(() => {
    setTimeout(() => {
      setHasCopied(false);
    }, 2000);
  }, []);

  function copyToClipboard() {
    if (data) {
      setHasCopied(true);
      navigator.clipboard.writeText(data.token);
    }
  }

  const form = useForm({
    defaultValues: {
      role: ROLE.Member as number,
    },
    validators: {
      onSubmit: InviteSchema,
    },
    onSubmit: async (values) => {
      await generateInvite({ role: values.value.role as BoardMemberRoleDto });
    },
  });

  return (
    <Popover>
      <Tooltip>
        <PopoverTrigger asChild>
          <TooltipTrigger asChild>
            <Button variant="ghost" size="icon">
              <span className="sr-only">Invite new member</span>
              <UserPlus2Icon className="size-4" />
            </Button>
          </TooltipTrigger>
        </PopoverTrigger>
        <TooltipContent>
          <p>Invite new member</p>
        </TooltipContent>
      </Tooltip>

      <PopoverContent align="end" className="flex w-[520px] flex-col gap-4">
        <div className="flex flex-col gap-1 text-center sm:text-left">
          <h3 className="text-lg font-semibold">Create Board Invite Link</h3>
          <p className="text-muted-foreground text-sm">
            Share this link to invite others to your board
          </p>
        </div>
        {data ? (
          <div className="relative flex-1">
            <Label htmlFor="link" className="sr-only">
              Link
            </Label>
            <Input
              id="link"
              defaultValue={data.token}
              readOnly
              className="h-9 pr-10"
            />

            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  type="button"
                  size="icon"
                  variant="ghost"
                  className="absolute top-1 right-1 size-7"
                  onClick={copyToClipboard}
                >
                  {hasCopied ? (
                    <>
                      <span className="sr-only">Copied</span>
                      <CheckIcon className="size-3.5" />
                    </>
                  ) : (
                    <>
                      <span className="sr-only">Copy</span>
                      <CopyIcon className="size-3.5" />
                    </>
                  )}
                </Button>
              </TooltipTrigger>
              <TooltipContent>{hasCopied ? "Copied" : "Copy"}</TooltipContent>
            </Tooltip>

            <Button
              type="button"
              onClick={() => {
                form.reset();
                reset();
              }}
              className="mt-4 w-full"
            >
              Generate another Invite Link
            </Button>
          </div>
        ) : (
          <form
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
              form.handleSubmit();
            }}
          >
            <FieldSet>
              <FieldGroup>
                <form.Field
                  name="role"
                  children={(field) => {
                    const isInvalid =
                      field.state.meta.isTouched && !field.state.meta.isValid;

                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Role</FieldLabel>
                        <Select
                          value={field.state.value.toString()}
                          onValueChange={(value) =>
                            field.handleChange(parseInt(value))
                          }
                        >
                          <SelectTrigger id={field.name}>
                            <SelectValue placeholder="Select role..." />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value={ROLE.Viewer.toString()}>
                              Viewer
                            </SelectItem>
                            <SelectItem value={ROLE.Member.toString()}>
                              Member
                            </SelectItem>
                            <SelectItem value={ROLE.Admin.toString()}>
                              Admin
                            </SelectItem>
                          </SelectContent>
                        </Select>

                        {isInvalid && (
                          <FieldError errors={field.state.meta.errors} />
                        )}
                      </Field>
                    );
                  }}
                />

                <Field>
                  <form.Subscribe
                    selector={(s) => !s.canSubmit && s.isSubmitting}
                  >
                    {(disabled) => (
                      <Button type="submit" disabled={disabled}>
                        {disabled ? "Generating..." : "Generate Invite Link"}
                      </Button>
                    )}
                  </form.Subscribe>
                </Field>
              </FieldGroup>
            </FieldSet>
          </form>
        )}
      </PopoverContent>
    </Popover>
  );
}
