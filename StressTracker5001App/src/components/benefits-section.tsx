import { CheckCircle, LayoutDashboard } from "lucide-react";

export function BenefitsSection() {
  return (
    <section id="benefits" className="py-20 sm:py-28">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
          <div className="animate-slide-in-left">
            <h2 className="text-3xl sm:text-4xl font-bold text-foreground mb-6 text-balance">
              Benefits
            </h2>
            <div className="space-y-6">
              {[
                {
                  title: "Clarity & Focus",
                  description:
                    "See exactly what needs to be done and eliminate decision fatigue with visual task management.",
                  delay: "delay-100",
                },
                {
                  title: "Reduced Overwhelm",
                  description:
                    "Break down complex projects into manageable tasks and track progress without stress.",
                  delay: "delay-200",
                },
                {
                  title: "Increased Productivity",
                  description:
                    "Stay organized and accomplish more with intuitive workflows that adapt to your needs.",
                  delay: "delay-300",
                },
                {
                  title: "Better Collaboration",
                  description:
                    "Keep everyone aligned with shared boards and real-time updates across your team.",
                  delay: "delay-400",
                },
              ].map((benefit, index) => (
                <div
                  key={index}
                  className={`flex gap-4 animate-fade-up ${benefit.delay}`}
                >
                  <div className="shrink-0">
                    <div className="h-10 w-10 rounded-full bg-foreground/5 flex items-center justify-center">
                      <CheckCircle className="h-5 w-5 text-foreground" />
                    </div>
                  </div>
                  <div>
                    <h3 className="text-lg font-semibold text-foreground mb-1">
                      {benefit.title}
                    </h3>
                    <p className="text-muted-foreground leading-relaxed">
                      {benefit.description}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="relative animate-slide-in-right">
            <LayoutDashboard className="size-full hover:scale-105 transition-transform duration-500" />

            {/* Decorative elements */}
            <div className="absolute -top-4 -right-4 w-24 h-24 border-2 border-foreground/10 rounded-2xl -z-10" />
            <div className="absolute -bottom-4 -left-4 w-32 h-32 border-2 border-foreground/10 rounded-2xl -z-10" />
          </div>
        </div>
      </div>
    </section>
  );
}
